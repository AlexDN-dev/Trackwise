namespace Trackwise.Application.Auth.Dtos;

public enum LoginError
{
    None,
    InvalidCredentials,
    EmailNotConfirmed
}

public record LoginResult(AuthResponse? Auth, LoginError Error);