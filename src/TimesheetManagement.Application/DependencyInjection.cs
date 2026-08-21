using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TimesheetManagement.Application.Options;
using TimesheetManagement.Application.Services;

namespace TimesheetManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TimesheetOptions>(configuration.GetSection(TimesheetOptions.SectionName));

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ITimesheetService, TimesheetService>();

        return services;
    }
}
