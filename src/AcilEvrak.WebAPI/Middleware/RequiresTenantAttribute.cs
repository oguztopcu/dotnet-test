namespace AcilEvrak.WebAPI.Middleware;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequiresTenantAttribute : Attribute;
