using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using PersonnelManager.Api.Auth;
using PersonnelManager.Api.Health;
using PersonnelManager.Api.Validation;
using PersonnelManager.Composition;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Configuration objects (validated on startup so misconfiguration fails fast).
// ---------------------------------------------------------------------------
builder.Services
    .AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<AuthOptions>()
    .Bind(builder.Configuration.GetSection(AuthOptions.SectionName));

// ---------------------------------------------------------------------------
// Core services (domain + application + EF/PostgreSQL or in-memory store).
// A connection string selects PostgreSQL; without one the in-memory store is used.
// ---------------------------------------------------------------------------
var dataDirectory = builder.Configuration["DataDirectory"];
if (string.IsNullOrWhiteSpace(dataDirectory))
    dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "data");
Directory.CreateDirectory(dataDirectory);

var connectionString = builder.Configuration.GetConnectionString("Personnel");
builder.Services.AddPersonnelManager(dataDirectory, connectionString);

// ---------------------------------------------------------------------------
// Authentication & authorization (JWT bearer, role-based).
// ---------------------------------------------------------------------------
builder.Services.AddSingleton<ITokenService, TokenService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                  ?? throw new InvalidOperationException("Missing 'Jwt' configuration section.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services.AddAuthorization();

// ---------------------------------------------------------------------------
// MVC controllers + FluentValidation at the HTTP boundary + RFC 7807 errors.
// ---------------------------------------------------------------------------
builder.Services
    .AddControllers(options => options.Filters.Add<FluentValidationFilter>())
    .AddJsonOptions(options =>
        // Serialize/accept EmploymentStatus as its name ("Active") rather than an integer.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddProblemDetails();

// ---------------------------------------------------------------------------
// Health checks.
// ---------------------------------------------------------------------------
builder.Services.AddHealthChecks().AddCheck<DataStoreHealthCheck>("data-store");

// ---------------------------------------------------------------------------
// OpenAPI document + Swagger UI (with a JWT "Authorize" button).
// ---------------------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PersonnelManager API",
        Version = "v1",
        Description = "REST API for personnel CRUD, status changes, search and JSON backup. JWT-secured, role-based.",
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the JWT from POST /api/auth/login (no 'Bearer ' prefix needed).",
    });
    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", null, null)] = new List<string>(),
    });

    var xmlPath = Path.Combine(AppContext.BaseDirectory,
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// ---------------------------------------------------------------------------
// HTTP pipeline.
// ---------------------------------------------------------------------------
app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "PersonnelManager API v1");
    options.DocumentTitle = "PersonnelManager API";
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var payload = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
            }),
        });
        await context.Response.WriteAsync(payload);
    },
}).AllowAnonymous();

app.Run();

// Exposed so integration tests (WebApplicationFactory<Program>) can bootstrap the API.
public partial class Program;
