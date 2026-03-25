using AcilEvrak.WebAPI.Extensions;
using AcilEvrak.WebAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMessaging(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddAuth(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.RunMigrations();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<TenantMiddleware>();
app.UseMiddleware<IdempotencyMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// WebApplicationFactory<Program> entegrasyon testlerinde bu sınıfa erişebilsin diye public yapıldı.
// Top-level statement'lar derleyici tarafından internal sınıf olarak üretilir;
// bu satır olmadan test projesi Program sınıfını göremez.
public partial class Program { }
