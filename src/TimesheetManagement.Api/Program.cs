using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TimesheetManagement.Api.Auth;
using TimesheetManagement.Api.Middleware;
using TimesheetManagement.Application;
using TimesheetManagement.Application.Auth;
using TimesheetManagement.Infrastructure;
using TimesheetManagement.Infrastructure.Auth;
using TimesheetManagement.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ---- Layers -----------------------------------------------------------
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// ---- Controllers / JSON ------------------------------------------------
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// ---- Swagger, with a bearer-token box so endpoints are testable in dev ----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Timesheet Management API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT bearer token. In Local mode, obtain one from POST /api/auth/local-login.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

// ---- CORS: allow the React dev server / configured customer origins ----
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
        }
    });
});

// ---- Auth: Auth:Provider picks Local vs EntraId. Controllers only ever see
// ---- [Authorize(Roles = ...)] and never know which mode issued the token. ----
var authProvider = builder.Configuration["Auth:Provider"] ?? "Local";
var authBuilder = builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);

if (string.Equals(authProvider, "EntraId", StringComparison.OrdinalIgnoreCase))
{
    var entra = builder.Configuration.GetSection(EntraIdOptions.SectionName).Get<EntraIdOptions>() ?? new EntraIdOptions();

    authBuilder.AddJwtBearer(options =>
    {
        options.Authority = $"{entra.Instance.TrimEnd('/')}/{entra.TenantId}/v2.0";
        options.Audience = string.IsNullOrWhiteSpace(entra.Audience) ? $"api://{entra.ClientId}" : entra.Audience;

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var provisioning = context.HttpContext.RequestServices.GetRequiredService<EntraJitProvisioningService>();
                var localUser = await provisioning.ProvisionOrGetAsync(context.Principal!, context.HttpContext.RequestAborted);

                var identity = (ClaimsIdentity)context.Principal!.Identity!;
                foreach (var stale in identity.FindAll(ClaimTypes.NameIdentifier).ToList()) identity.RemoveClaim(stale);
                foreach (var stale in identity.FindAll(ClaimTypes.Role).ToList()) identity.RemoveClaim(stale);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, localUser.Id.ToString()));
                identity.AddClaim(new Claim(ClaimTypes.Role, localUser.Role.ToString()));
            }
        };
    });
}
else
{
    var local = builder.Configuration.GetSection(LocalAuthOptions.SectionName).Get<LocalAuthOptions>() ?? new LocalAuthOptions();
    var signingKey = builder.Configuration["Auth:Local:SigningKey"];

    authBuilder.AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = local.Issuer,
            ValidateAudience = true,
            ValidAudience = local.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(string.IsNullOrEmpty(signingKey) ? "dev-only-placeholder-key-set-via-user-secrets-0000" : signingKey))
        };
    });
}

builder.Services.AddAuthorization();

// ---- Application Insights: no-ops locally when ConnectionString is blank ----
builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

// Local dev convenience: apply migrations (incl. seed data) on startup so `dotnet run` alone gets you a working DB.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<TimesheetDbContext>();
    await db.Database.MigrateAsync();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
