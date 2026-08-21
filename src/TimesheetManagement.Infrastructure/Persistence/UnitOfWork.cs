using TimesheetManagement.Application.Repositories;

namespace TimesheetManagement.Infrastructure.Persistence;

public class UnitOfWork(TimesheetDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => dbContext.SaveChangesAsync(ct);
}
