using TimesheetManagement.Application.Repositories;
using TimesheetManagement.Domain.Entities;
using TimesheetManagement.Infrastructure.Persistence;

namespace TimesheetManagement.Infrastructure.Repositories;

public class ApprovalHistoryRepository(TimesheetDbContext db) : IApprovalHistoryRepository
{
    public async Task AddAsync(ApprovalHistory history, CancellationToken ct = default) =>
        await db.ApprovalHistories.AddAsync(history, ct);
}
