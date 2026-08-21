using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TimesheetManagement.Application.Auth;
using TimesheetManagement.Application.Repositories;
using TimesheetManagement.Infrastructure.Auth;
using TimesheetManagement.Infrastructure.Persistence;
using TimesheetManagement.Infrastructure.Repositories;

namespace TimesheetManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TimesheetDbContext>(options =>
            options.UseSqlServer(BuildConnectionString(configuration)));

        services.Configure<LocalAuthOptions>(configuration.GetSection(LocalAuthOptions.SectionName));
        services.Configure<EntraIdOptions>(configuration.GetSection(EntraIdOptions.SectionName));
        services.AddScoped<EntraJitProvisioningService>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IProjectTaskRepository, ProjectTaskRepository>();
        services.AddScoped<ITimesheetWeekRepository, TimesheetWeekRepository>();
        services.AddScoped<IApprovalHistoryRepository, ApprovalHistoryRepository>();
        services.AddScoped<ILocalAuthTokenService, LocalAuthTokenService>();

        return services;
    }

    /// <summary>
    /// ConnectionStrings:TimesheetDb holds everything except the password (server, database, user id) so it can
    /// stay in appsettings.*.json. The password is layered in from "Db:Password" — user-secrets in Development,
    /// an environment variable or Key Vault reference in any shared environment — and is never checked in.
    /// </summary>
    private static string BuildConnectionString(IConfiguration configuration)
    {
        var baseConnectionString = configuration.GetConnectionString("TimesheetDb")
            ?? throw new InvalidOperationException("ConnectionStrings:TimesheetDb is not configured.");

        var password = configuration["Db:Password"];
        if (string.IsNullOrEmpty(password))
        {
            return baseConnectionString;
        }

        var builder = new SqlConnectionStringBuilder(baseConnectionString) { Password = password };
        return builder.ConnectionString;
    }
}
