using TimesheetManagement.Domain.Entities;

namespace TimesheetManagement.Application.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByExternalAuthIdAsync(string externalAuthId, CancellationToken ct = default);
    Task<List<User>> GetAllAsync(CancellationToken ct = default);
    Task<List<User>> GetDirectReportsAsync(Guid managerId, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
}
