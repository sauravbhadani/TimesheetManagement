using Microsoft.EntityFrameworkCore;
using TimesheetManagement.Application.Repositories;
using TimesheetManagement.Domain.Entities;
using TimesheetManagement.Infrastructure.Persistence;

namespace TimesheetManagement.Infrastructure.Repositories;

public class UserRepository(TimesheetDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByExternalAuthIdAsync(string externalAuthId, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(u => u.ExternalAuthId == externalAuthId, ct);

    public Task<List<User>> GetAllAsync(CancellationToken ct = default) =>
        db.Users.AsNoTracking().OrderBy(u => u.FullName).ToListAsync(ct);

    public Task<List<User>> GetDirectReportsAsync(Guid managerId, CancellationToken ct = default) =>
        db.Users.Where(u => u.ManagerId == managerId).ToListAsync(ct);

    public async Task AddAsync(User user, CancellationToken ct = default) =>
        await db.Users.AddAsync(user, ct);
}
