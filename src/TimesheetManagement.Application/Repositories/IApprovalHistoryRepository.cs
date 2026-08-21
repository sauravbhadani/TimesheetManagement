using TimesheetManagement.Domain.Entities;

namespace TimesheetManagement.Application.Repositories;

public interface IApprovalHistoryRepository
{
    Task AddAsync(ApprovalHistory history, CancellationToken ct = default);
}
