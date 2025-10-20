using AttaEduSystem.API.Extension;
using AttaEduSystem.API.Middleware;
using AttaEduSystem.DataAccess.DBContext;
using AttaEduSystem.Models.Entities;
using AttaEduSystem.Utilities.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Configure DbContext with SQL Server  
builder.Services.AddDbContext<ApplicationDBContext>(options =>
{
    //options.UseSqlServer(
    //    builder.Configuration.GetConnectionString(StaticConnectionString.SqldbDefaultConnection));
    options.UseNpgsql(
        builder.Configuration.GetConnectionString(StaticConnectionString.PostgreSqlConnection));
});

// Configure Identity  
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDBContext>()
    .AddDefaultTokenProviders();

// Thêm d?ch v? Swagger  
builder.Services.AddSwaggerGen(options =>
{
    // B?o m?t Swagger v?i JWT
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Please enter your token with this format: \"Bearer YOUR_TOKEN\""
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new List<string>()
                }
            });

    // API document
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AttaEdu System API",
        Version = "v1",
        Description = "API documentation for AttaEdu System"
    });
    options.EnableAnnotations();

    // ??c comment t? XML ?? hi?n th? trên Swagger
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);
});

// Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        ValidAudience = builder.Configuration["JWT:ValidAudience"],
        IssuerSigningKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
    };
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register services from Extensions
builder.Services.RegisterServices(builder.Configuration);

// Configure CORS
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAttaEduSystem",
        builder =>
        {
            builder.WithOrigins(corsOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

var app = builder.Build();

// Setup CORS middleware
app.Use(async (context, next) =>
{
    var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("CORSMiddleware");

    logger.LogInformation($"Request from origin: {context.Request.Headers["Origin"]}");
    logger.LogInformation($"Request method: {context.Request.Method}");
    logger.LogInformation($"Request path: {context.Request.Path}");

    if (context.Request.Method == "OPTIONS")
    {
        context.Response.Headers.Append("Access-Control-Allow-Origin", context.Request.Headers["Origin"]);
        context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Accept, Authorization, x-requested-with, x-signalr-user-agent");
        context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
        context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
        context.Response.StatusCode = 200;
        await context.Response.CompleteAsync();
    }
    else
    {
        await next();
    }

    if (context.Response.Headers.ContainsKey("Access-Control-Allow-Origin"))
        logger.LogInformation(
            $"Response Access-Control-Allow-Origin: {context.Response.Headers["Access-Control-Allow-Origin"]}");
});

app.UseCors("AllowAttaEduSystem");
app.UseMiddleware<GlobalExceptionHandllingMiddleware>();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
