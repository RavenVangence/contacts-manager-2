using ContactsManager.Server.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Runtime.InteropServices;

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

// Create cancellation token source for graceful shutdown
var shutdownTokenSource = new CancellationTokenSource();

// Start the application in the background
var runTask = app.RunAsync(shutdownTokenSource.Token);

// Open browser and monitor for closure
_ = Task.Run(async () =>
{
    await Task.Delay(2000); // Wait 2 seconds for server to start
    var url = "http://localhost:5000";
    Console.WriteLine($"Opening browser at: {url}");

    var browserProcess = OpenBrowser(url);

    if (browserProcess != null)
    {
        Console.WriteLine("Monitoring browser process. Server will close when browser is closed.");

        // Monitor browser process in background
        _ = Task.Run(async () =>
        {
            try
            {
                await browserProcess.WaitForExitAsync();
                Console.WriteLine("Browser closed. Shutting down server...");
                shutdownTokenSource.Cancel();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error monitoring browser: {ex.Message}");
            }
        });
    }
    else
    {
        Console.WriteLine("Could not monitor browser process. Server will remain running.");
        Console.WriteLine("Press Ctrl+C to stop the server manually.");
    }
});

// Wait for the application to complete
try
{
    await runTask;
}
catch (OperationCanceledException)
{
    Console.WriteLine("Server shutdown completed.");
}

// Method to open browser cross-platform and return process
static Process? OpenBrowser(string url)
{
    try
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Start browser process that we can monitor
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd",
                Arguments = $"/c start \"\" \"{url}\"",
                CreateNoWindow = true,
                UseShellExecute = false
            };

            var cmdProcess = Process.Start(startInfo);

            // Wait a moment for the browser to start
            Task.Delay(1000).Wait();

            // Find the browser process - look for common browser process names
            var browserProcesses = new List<Process>();
            var browserNames = new[] { "chrome", "firefox", "msedge", "opera", "brave", "safari" };

            foreach (var browserName in browserNames)
            {
                try
                {
                    var processes = Process.GetProcessesByName(browserName);
                    if (processes.Length > 0)
                    {
                        // Return the most recently started process
                        browserProcesses.AddRange(processes);
                    }
                }
                catch { }
            }

            if (browserProcesses.Count > 0)
            {
                // Return the most recently started browser process
                var latestProcess = browserProcesses.OrderByDescending(p =>
                {
                    try { return p.StartTime; }
                    catch { return DateTime.MinValue; }
                }).FirstOrDefault();

                Console.WriteLine($"Successfully opened browser for: {url}");
                return latestProcess;
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var process = Process.Start("open", url);
            Console.WriteLine($"Successfully opened browser for: {url}");
            return process;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var process = Process.Start("xdg-open", url);
            Console.WriteLine($"Successfully opened browser for: {url}");
            return process;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Could not open browser: {ex.Message}");
        Console.WriteLine($"Please manually open: {url}");
    }

    return null;
}
