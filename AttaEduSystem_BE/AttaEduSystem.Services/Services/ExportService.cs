using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.DTOs.Export;
using AttaEduSystem.Models.DTOs.GeminiAi;
using AttaEduSystem.Services.IServices;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;

// QuestPDF
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

// OpenXml - dùng alias để tránh conflict
using OpenXmlDocument = DocumentFormat.OpenXml.Packaging.WordprocessingDocument;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;

namespace AttaEduSystem.Services.Services;

public class ExportService : IExportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExportService> _logger;

    public ExportService(
        IUnitOfWork unitOfWork,
        IHttpClientFactory httpClientFactory,
        ILogger<ExportService> logger)
    {
        _unitOfWork = unitOfWork;
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;
    }

    public async Task<(byte[]? PdfBytes, string? ErrorMessage)> ExportToPdfAsync(
        Guid examId,
        ExportPdfRequestDto request,
        ClaimsPrincipal user)
    {
        try
        {
            // Get current user info
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = user.FindFirstValue("FullName")
                ?? user.FindFirstValue(ClaimTypes.Name)
                ?? "Unknown";

            ExamPdfData? pdfData = request.Source.ToLower() switch
            {
                "exampaper" => await GetExamPaperDataAsync(examId),
                "generatedexam" => await GetGeneratedExamDataAsync(examId),
                _ => null
            };

            if (pdfData == null)
            {
                return (null, $"{request.Source} with ID {examId} not found");
            }

            var pdfBytes = await GeneratePdfAsync(pdfData, request.IncludeAnswers);

            //if (request.UploadToCloud)
            //{
            //    var fileName = $"{SanitizeFileName(pdfData.Title)}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
            //    var url = await _cloudinaryService.UploadRawFileAsync(
            //        pdfBytes,
            //        fileName,
            //        StaticCloudinaryFolders.ExportedPdfs);

            //    var response = new ExportPdfResponseDto
            //    {
            //        DownloadUrl = url,
            //        FileName = fileName,
            //        FileSizeBytes = pdfBytes.Length,
            //        ExportedBy = userName,
            //        ExportedAt = DateTime.UtcNow
            //    };

            //    return (pdfBytes, null);
            //}

            return (pdfBytes, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting PDF for {Source} {ExamId}", request.Source, examId);
            return (null, $"Export failed: {ex.Message}");
        }
    }

    public async Task<(byte[]? WordBytes, string? ErrorMessage)> ExportToWordAsync(
    Guid examId,
    ExportPdfRequestDto request,
    ClaimsPrincipal user)
    {
        try
        {
            ExamPdfData? examData = request.Source.ToLower() switch
            {
                "exampaper" => await GetExamPaperDataAsync(examId),
                "generatedexam" => await GetGeneratedExamDataAsync(examId),
                _ => null
            };

            if (examData == null)
            {
                return (null, $"{request.Source} with ID {examId} not found");
            }

            var wordBytes = GenerateWordDocument(examData, request.IncludeAnswers);

            return (wordBytes, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting Word for {Source} {ExamId}", request.Source, examId);
            return (null, $"Export failed: {ex.Message}");
        }
    }

    // =========================================================
    // GET DATA METHODS
    // =========================================================
    private async Task<ExamPdfData?> GetExamPaperDataAsync(Guid examId)
    {
        var examPaper = await _unitOfWork.ExamPaper.GetAsync(
            e => e.ExamPaperId == examId,
            includeProperties: "Questions,Questions.Options");

        if (examPaper == null) return null;

        return new ExamPdfData
        {
            Title = examPaper.Title,
            Subject = examPaper.Subject ?? "Unknown",
            Description = examPaper.Description,
            Questions = examPaper.Questions?
                .OrderBy(q => q.OrderIndex)
                .Select(q => new ExamQuestionPdfData
                {
                    QuestionNumber = q.OrderIndex,
                    Content = q.Content,
                    Options = q.Options?
                        .OrderBy(o => o.Label)
                        .Select(o => $"{o.Label}. {o.Content}")
                        .ToList() ?? new List<string>(),
                    CorrectAnswer = q.CorrectAnswer,
                    QuestionType = q.QuestionType ?? "MultipleChoice"
                }).ToList() ?? new List<ExamQuestionPdfData>()
        };
    }

    private async Task<ExamPdfData?> GetGeneratedExamDataAsync(Guid examId)
    {
        var generated = await _unitOfWork.GeneratedExamPaper.GetAsync(
            g => g.GeneratedExamPaperId == examId,
            includeProperties: "OriginalExamPaper");

        if (generated == null) return null;

        try
        {
            var parsed = JsonSerializer.Deserialize<ExamStructureResponse>(
                generated.GeneratedContentJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // ExamInfo is Dictionary<string, string>, access by key
            var title = GetDictionaryValue(parsed?.ExamInfo, "title")
                ?? generated.OriginalExamPaper?.Title
                ?? "Generated Exam";

            var subject = GetDictionaryValue(parsed?.ExamInfo, "suggested_subject") ?? "Unknown";

            var description = generated.OriginalExamPaper?.Title != null
                ? $"AI Generated from: {generated.OriginalExamPaper.Title}"
                : "AI Generated Exam";

            return new ExamPdfData
            {
                Title = title,
                Subject = subject,
                Description = description,
                Questions = parsed?.Questions?
                    .Select((q, index) => new ExamQuestionPdfData
                    {
                        QuestionNumber = index + 1,
                        Content = q.Content,
                        Options = q.Options?.ToList() ?? new List<string>(),
                        CorrectAnswer = null,
                        QuestionType = q.Type ?? "MultipleChoice"
                    }).ToList() ?? new List<ExamQuestionPdfData>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse GeneratedExamPaper JSON");
            return null;
        }
    }

    // =========================================================
    // PDF GENERATION
    // =========================================================
    private async Task<byte[]> GeneratePdfAsync(ExamPdfData data, bool includeAnswers)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var latexImages = await PreFetchLatexImagesAsync(data);

        var document = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginTop(1.5f, Unit.Centimetre);
                page.MarginBottom(1.5f, Unit.Centimetre);
                page.MarginHorizontal(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header().Element(c => ComposeHeader(c, data));
                page.Content().Element(c => ComposeContent(c, data, latexImages, includeAnswers));

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, ExamPdfData data)
    {
        container.Column(column =>
        {
            column.Item().BorderBottom(1).BorderColor(Colors.Grey.Medium).PaddingBottom(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("EXAM").Bold().FontSize(18).FontColor(Colors.Blue.Darken2);
                    col.Item().Text(data.Title).SemiBold().FontSize(14);
                });

                row.ConstantItem(150).AlignRight().Column(col =>
                {
                    col.Item().Text($"Subject: {data.Subject}").FontSize(10);
                    col.Item().Text($"Date: {DateTime.Now:dd/MM/yyyy}").FontSize(10);
                });
            });

            if (!string.IsNullOrEmpty(data.Description))
            {
                column.Item().PaddingTop(5).Text(data.Description).Italic().FontSize(10).FontColor(Colors.Grey.Darken1);
            }
        });
    }

    private void ComposeContent(IContainer container, ExamPdfData data, Dictionary<string, byte[]> latexImages, bool includeAnswers)
    {
        container.PaddingTop(15).Column(column =>
        {
            foreach (var question in data.Questions)
            {
                column.Item().Element(c => ComposeQuestion(c, question, latexImages));
            }

            if (includeAnswers && data.Questions.Any(q => !string.IsNullOrEmpty(q.CorrectAnswer)))
            {
                column.Item().PageBreak();
                column.Item().Element(c => ComposeAnswerKey(c, data.Questions));
            }
        });
    }

    private void ComposeQuestion(IContainer container, ExamQuestionPdfData question, Dictionary<string, byte[]> latexImages)
    {
        container.PaddingBottom(15).Column(column =>
        {
            column.Item().Row(row =>
            {
                row.ConstantItem(50).Text($"Q{question.QuestionNumber}:").Bold();
                row.RelativeItem().Element(c => RenderTextWithLatex(c, question.Content, latexImages));
            });

            if (question.QuestionType == "MultipleChoice" && question.Options.Any())
            {
                column.Item().PaddingLeft(50).PaddingTop(5).Column(optCol =>
                {
                    foreach (var option in question.Options)
                    {
                        optCol.Item().PaddingBottom(3).Element(c => RenderTextWithLatex(c, option, latexImages));
                    }
                });
            }
            else if (question.QuestionType == "Essay")
            {
                column.Item().PaddingLeft(50).PaddingTop(5)
                    .Border(1).BorderColor(Colors.Grey.Lighten1)
                    .Height(60)
                    .AlignMiddle().AlignCenter()
                    .Text("(Answer area)").FontSize(9).FontColor(Colors.Grey.Medium);
            }
        });
    }

    private void ComposeAnswerKey(IContainer container, List<ExamQuestionPdfData> questions)
    {
        container.Column(column =>
        {
            column.Item().Text("ANSWER KEY").Bold().FontSize(16).FontColor(Colors.Blue.Darken2);
            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.ConstantColumn(80);
                    cols.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Question").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Answer").Bold();
                });

                foreach (var q in questions.Where(q => !string.IsNullOrEmpty(q.CorrectAnswer)))
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                        .Text($"Q{q.QuestionNumber}");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                        .Text(q.CorrectAnswer ?? "-");
                }
            });
        });
    }

    // =========================================================
    // WORD GENERATION
    // =========================================================
    private byte[] GenerateWordDocument(ExamPdfData data, bool includeAnswers)
    {
        using var stream = new MemoryStream();

        using (var wordDocument = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
        {
            var mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
            var body = mainPart.Document.AppendChild(new Body());

            // Title
            AddParagraph(body, "EXAM", true, "28", JustificationValues.Center);
            AddParagraph(body, data.Title, true, "24", JustificationValues.Center);

            // Info
            AddParagraph(body, $"Subject: {data.Subject}", false, "20", JustificationValues.Left);
            AddParagraph(body, $"Date: {DateTime.Now:dd/MM/yyyy}", false, "20", JustificationValues.Left);

            if (!string.IsNullOrEmpty(data.Description))
            {
                AddParagraph(body, data.Description, false, "18", JustificationValues.Left, true);
            }

            // Separator
            AddParagraph(body, "═══════════════════════════════════════", false, "20", JustificationValues.Center);

            // Questions
            foreach (var question in data.Questions)
            {
                // Question content
                AddParagraph(body, $"Q{question.QuestionNumber}: {question.Content}", true, "22", JustificationValues.Left);

                // Options
                if (question.QuestionType == "MultipleChoice" && question.Options.Any())
                {
                    foreach (var option in question.Options)
                    {
                        AddParagraph(body, $"    {option}", false, "20", JustificationValues.Left);
                    }
                }
                else if (question.QuestionType == "Essay")
                {
                    AddParagraph(body, "    Answer: _______________________________________________", false, "20", JustificationValues.Left);
                    AddParagraph(body, "", false, "20", JustificationValues.Left);
                    AddParagraph(body, "    _______________________________________________", false, "20", JustificationValues.Left);
                }

                // Spacing
                AddParagraph(body, "", false, "12", JustificationValues.Left);
            }

            // Answer Key
            if (includeAnswers && data.Questions.Any(q => !string.IsNullOrEmpty(q.CorrectAnswer)))
            {
                // Page break
                body.AppendChild(new Paragraph(new Run(new Break { Type = BreakValues.Page })));

                AddParagraph(body, "ANSWER KEY", true, "28", JustificationValues.Center);
                AddParagraph(body, "═══════════════════════════════════════", false, "20", JustificationValues.Center);

                foreach (var q in data.Questions.Where(q => !string.IsNullOrEmpty(q.CorrectAnswer)))
                {
                    AddParagraph(body, $"Q{q.QuestionNumber}: {q.CorrectAnswer}", false, "20", JustificationValues.Left);
                }
            }

            mainPart.Document.Save();
        }

        return stream.ToArray();
    }

    private void AddParagraph(Body body, string text, bool bold, string fontSize, JustificationValues justification, bool italic = false)
    {
        var paragraph = new Paragraph();
        var paragraphProperties = new ParagraphProperties
        {
            Justification = new Justification { Val = justification }
        };
        paragraph.AppendChild(paragraphProperties);

        var run = new Run();
        var runProperties = new RunProperties();

        if (bold) runProperties.AppendChild(new Bold());
        if (italic) runProperties.AppendChild(new Italic());
        runProperties.AppendChild(new FontSize { Val = fontSize });
        runProperties.AppendChild(new RunFonts { Ascii = "Arial", HighAnsi = "Arial" });

        run.AppendChild(runProperties);
        run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });

        paragraph.AppendChild(run);
        body.AppendChild(paragraph);
    }

    // =========================================================
    // LATEX RENDERING (CodeCogs API)
    // =========================================================
    private async Task<Dictionary<string, byte[]>> PreFetchLatexImagesAsync(ExamPdfData data)
    {
        var latexImages = new Dictionary<string, byte[]>();
        var latexPattern = new Regex(@"\$([^$]+)\$");

        var allTexts = new List<string> { data.Title, data.Description ?? "" };
        allTexts.AddRange(data.Questions.Select(q => q.Content));
        allTexts.AddRange(data.Questions.SelectMany(q => q.Options));

        var latexExpressions = allTexts
            .Where(text => !string.IsNullOrEmpty(text))
            .SelectMany(text => latexPattern.Matches(text).Select(m => m.Groups[1].Value))
            .Distinct()
            .ToList();

        var tasks = latexExpressions.Select(async latex =>
        {
            try
            {
                var imageBytes = await FetchLatexImageAsync(latex);
                return (latex, imageBytes);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch LaTeX image for: {Latex}", latex);
                return (latex, (byte[]?)null);
            }
        });

        var results = await Task.WhenAll(tasks);

        foreach (var (latex, imageBytes) in results)
        {
            if (imageBytes != null)
            {
                latexImages[latex] = imageBytes;
            }
        }

        return latexImages;
    }

    private async Task<byte[]?> FetchLatexImageAsync(string latex)
    {
        var encodedLatex = Uri.EscapeDataString(latex);
        var url = $"https://latex.codecogs.com/png.latex?\\dpi{{150}}{encodedLatex}";

        var response = await _httpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsByteArrayAsync();
        }

        return null;
    }

    private void RenderTextWithLatex(IContainer container, string text, Dictionary<string, byte[]> latexImages)
    {
        if (string.IsNullOrEmpty(text))
        {
            container.Text("");
            return;
        }

        var latexPattern = new Regex(@"\$([^$]+)\$");
        var matches = latexPattern.Matches(text);

        if (!matches.Any())
        {
            container.Text(text);
            return;
        }

        container.Row(row =>
        {
            int lastIndex = 0;

            foreach (Match match in matches)
            {
                if (match.Index > lastIndex)
                {
                    var beforeText = text.Substring(lastIndex, match.Index - lastIndex);
                    row.AutoItem().AlignMiddle().Text(beforeText);
                }

                var latex = match.Groups[1].Value;
                if (latexImages.TryGetValue(latex, out var imageBytes))
                {
                    row.AutoItem().AlignMiddle().Height(20).Image(imageBytes, ImageScaling.FitHeight);
                }
                else
                {
                    row.AutoItem().AlignMiddle().Text($"${latex}$").Italic().FontColor(Colors.Grey.Darken1);
                }

                lastIndex = match.Index + match.Length;
            }

            if (lastIndex < text.Length)
            {
                row.AutoItem().AlignMiddle().Text(text.Substring(lastIndex));
            }
        });
    }

    // =========================================================
    // HELPERS
    // =========================================================
    private static string? GetDictionaryValue(Dictionary<string, string>? dict, string key)
    {
        if (dict == null) return null;
        return dict.TryGetValue(key, out var value) ? value : null;
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
    }
}

// =========================================================
// INTERNAL DTOs FOR PDF GENERATION
// =========================================================
internal class ExamPdfData
{
    public string Title { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string? Description { get; set; }
    public List<ExamQuestionPdfData> Questions { get; set; } = new();
}

internal class ExamQuestionPdfData
{
    public int QuestionNumber { get; set; }
    public string Content { get; set; } = null!;
    public List<string> Options { get; set; } = new();
    public string? CorrectAnswer { get; set; }
    public string QuestionType { get; set; } = "MultipleChoice";
}