namespace AcilEvrak.WebAPI.Controllers;

public sealed record LoginRequest(string Email, string Password, string? DeviceName);
