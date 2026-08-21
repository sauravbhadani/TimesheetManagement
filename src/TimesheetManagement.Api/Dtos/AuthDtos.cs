namespace TimesheetManagement.Api.Dtos;

public record LocalUserOptionDto(Guid Id, string FullName, string Email, string Role);

public record LocalLoginRequest(Guid UserId);

public record LocalLoginResponse(string Token, LocalUserOptionDto User);

public record CurrentUserDto(Guid Id, string FullName, string Email, string Role);
