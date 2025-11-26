using AttaEduSystem.Models.DTOs.ExamFormat;
using AttaEduSystem.Services.IServices;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AttaEduSystem.Services.Services
{
    public class RegexExamFormatParser : IExamFormatParser
    {
        private static readonly Regex TitleRegex = new(@"^(ĐỀ .+|KIỂM TRA .+|BÀI KIỂM TRA .+)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        private static readonly Regex SectionRegex = new(@"(?<=\n)(I{1,4}|V|VI|VII)\.\s*(.+)", RegexOptions.IgnoreCase);
        private static readonly Regex QuestionRegex = new(@"(?<=\n)(Câu\s*\d+|Question\s*\d+)\s*(\([\d,\.]+\s*(điểm|diem)\))?(.*)", RegexOptions.IgnoreCase);
        private static readonly Regex SubQuestionRegex = new(@"(\d\.\d|\([a-z]\))\s*(.+)");
        private static readonly Regex PointRegex = new(@"\(?([\d,\.]+)\s*(điểm|diem)\)?", RegexOptions.IgnoreCase);
        private static readonly Regex TimeRegex = new(@"Thời gian:?\s*(.+)", RegexOptions.IgnoreCase);
        private static readonly Regex SubjectRegex = new(@"(Môn|Subject):?\s*(.+)", RegexOptions.IgnoreCase);
        private static readonly Regex InstructionRegex = new(@"Thực hiện các yêu cầu.+|Học sinh.+", RegexOptions.IgnoreCase);

        public ExamFormatSchema Parse(string scannedText)
        {
            var text = Normalize(scannedText);

            var schema = new ExamFormatSchema
            {
                Title = ExtractTitle(text),
                Subject = DetectSubject(text),
                Instructions = ExtractInstructions(text),
                TimeLimit = ExtractTime(text)
            };

            var sections = ExtractSections(text).ToList();
            if (!sections.Any())
            {
                sections.Add(ParseDefaultSection(text));
            }

            schema.Sections = sections;
            return schema;
        }

        private string Normalize(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

            var builder = new StringBuilder(raw);
            builder.Replace("\r", "\n");
            builder.Replace("\n\n", "\n");
            return builder.ToString().Trim();
        }

        private string ExtractTitle(string text)
            => TitleRegex.Match(text).Value.Trim();

        private string DetectSubject(string text)
            => SubjectRegex.Match(text) is { Success: true } m ? m.Groups[2].Value.Trim() : string.Empty;

        private string? ExtractTime(string text)
            => TimeRegex.Match(text) is { Success: true } m ? m.Groups[1].Value.Trim() : null;

        private string? ExtractInstructions(string text)
        {
            var instruction = InstructionRegex.Match(text);
            if (instruction.Success)
            {
                var end = text.IndexOf("Câu", instruction.Index, StringComparison.OrdinalIgnoreCase);
                if (end > instruction.Index)
                {
                    return text.Substring(instruction.Index, end - instruction.Index).Trim();
                }
                return text[instruction.Index..].Trim();
            }

            var firstSection = SectionRegex.Match(text);
            if (firstSection.Success && firstSection.Index > 0)
            {
                return text[..firstSection.Index].Trim();
            }

            var firstQuestion = QuestionRegex.Match(text);
            return firstQuestion.Success && firstQuestion.Index > 0
                ? text[..firstQuestion.Index].Trim()
                : null;
        }

        private IEnumerable<ExamSection> ExtractSections(string text)
        {
            var matches = SectionRegex.Matches(text);
            if (matches.Count == 0) yield break;

            for (int i = 0; i < matches.Count; i++)
            {
                var start = matches[i].Index;
                var end = i + 1 < matches.Count ? matches[i + 1].Index : text.Length;
                var sectionBody = text.Substring(start, end - start);

                yield return new ExamSection
                {
                    Name = matches[i].Groups[2].Value.Trim(),
                    TotalPoints = SumPoints(sectionBody),
                    Questions = ExtractQuestions(sectionBody).ToList()
                };
            }
        }

        private ExamSection ParseDefaultSection(string text) => new()
        {
            Name = "Phần chung",
            TotalPoints = SumPoints(text),
            Questions = ExtractQuestions(text).ToList()
        };

        private IEnumerable<ExamQuestion> ExtractQuestions(string text)
        {
            var matches = QuestionRegex.Matches(text);
            foreach (Match match in matches)
            {
                var code = match.Groups[1].Value.Trim();
                var remainder = match.Groups[4].Value.Trim();
                var subQuestions = ExtractSubQuestions(remainder);

                yield return new ExamQuestion
                {
                    Code = code,
                    Points = ParsePoint(match.Value),
                    Requirement = subQuestions.MainRequirement,
                    SubRequirement = subQuestions.SubRequirement
                };
            }
        }

        private (string MainRequirement, string? SubRequirement) ExtractSubQuestions(string requirement)
        {
            var match = SubQuestionRegex.Match(requirement);
            if (!match.Success)
            {
                return (requirement.Trim(), null);
            }

            var main = requirement[..match.Index].Trim();
            var sub = requirement[match.Index..].Trim();
            return (main, sub);
        }

        private decimal? ParsePoint(string text)
        {
            var match = PointRegex.Match(text);
            if (!match.Success) return null;

            var raw = match.Groups[1].Value.Replace(',', '.');
            return decimal.TryParse(raw, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value)
                ? value
                : null;
        }

        private decimal? SumPoints(string text)
        {
            var matches = PointRegex.Matches(text);
            if (matches.Count == 0) return null;

            decimal total = 0;
            foreach (Match match in matches)
            {
                var raw = match.Groups[1].Value.Replace(',', '.');
                if (decimal.TryParse(raw, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value))
                {
                    total += value;
                }
            }
            return total;
        }
    }
}
