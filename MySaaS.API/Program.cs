using MySaaS.Infrastructure;
using MySaaS.API.Middleware;

var builder = WebApplication.CreateBuilder(args);


// ====================================================
// 1. REGISTER SERVICES (Dependency Injection)
// ====================================================

// A. Plug in the Infrastructure Layer (Database & Identity)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add Exception Handler (must be before controllers)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// B. Add API Controllers
builder.Services.AddControllers();

// C. Add CORS for Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                builder.Configuration["Frontend:Url"] ?? "http://localhost:5173",
                "http://localhost:3000" // Backup React default port
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// D. Add Swagger (so we can test the API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Use exception handler (replaces middleware)
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS before authentication
app.UseCors("AllowFrontend");

// These two must be in this order
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();

