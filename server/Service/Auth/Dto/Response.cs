namespace Service.Auth.Dto;

public record RegisterResponse(string Email, string Name);

public record LoginResponse;

public record AuthUserInfo(string Username, bool IsAdmin, bool CanPublish);