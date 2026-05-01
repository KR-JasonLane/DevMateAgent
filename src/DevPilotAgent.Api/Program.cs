using DevPilotAgent.Application;
using DevPilotAgent.Application.Interfaces;
using DevPilotAgent.Infrastructure;
using DevPilotAgent.Infrastructure.Agent;
using DevPilotAgent.Infrastructure.Persistence;
using DevPilotAgent.Api.Hubs;
using DevPilotAgent.Api.Middleware;
using DevPilotAgent.Api.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/devpilotagent-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// EF Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Database")));

// Application + Infrastructure DI
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddAgentServices(builder.Configuration);

// SignalR (메시지 크기 제한 확장)
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 256 * 1024;
});

// SignalRProgressReporter (Api 프로젝트에서 등록 - 레이어 역의존 방지)
builder.Services.AddScoped<IAgentProgressReporter, SignalRProgressReporter>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS (BlazorWebView 출처 명시 + AllowCredentials)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("https://0.0.0.0", "app://localhost", "http://localhost:5000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

var app = builder.Build();

// 자동 마이그레이션 (개발 편의)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseMiddleware<ExceptionMiddleware>();
app.MapControllers();
app.MapHub<AnalysisHub>("/hubs/analysis");

// Health Check
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();

// WebApplicationFactory에서 접근하기 위한 partial class
public partial class Program;
