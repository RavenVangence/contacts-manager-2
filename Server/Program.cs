using ContactsManager.Server.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add logging to see console output
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

builder.Services.AddSingleton<IContactRepository, ExcelContactRepository>();
builder.Services.AddControllers();
builder.Services.AddCors(o => o.AddPolicy("ui", p => p
    .WithOrigins("http://localhost:5173", "http://localhost:3000")
    .AllowAnyHeader()
    .AllowAnyMethod()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("ui");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Serve static files (React build output)
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

// Fallback for client-side routing
app.MapFallbackToFile("index.html");

app.Run();
