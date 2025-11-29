using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Logging;
using Win32Emu.Wasm;
using Win32Emu.Wasm.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure logging to enable debug verbosity for Win32 module logs
// In production, you might want to set this to Warning or Error
// For debugging purposes, we keep it at Debug to capture all messages
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Add browser console logging with detailed formatting
builder.Logging.AddFilter("Microsoft.AspNetCore.Components", LogLevel.Warning);
builder.Logging.AddFilter("Win32Emu", LogLevel.Debug);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register the EmulatorService for dependency injection
builder.Services.AddScoped<EmulatorService>();

var host = builder.Build();

// Log startup information
var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Win32Emu.Wasm");
logger.LogInformation("Win32Emu WASM starting...");
logger.LogInformation("Environment: {Environment}", builder.HostEnvironment.Environment);
logger.LogInformation("Base Address: {BaseAddress}", builder.HostEnvironment.BaseAddress);

try
{
	await host.RunAsync();
}
catch (Exception ex)
{
	logger.LogCritical(ex, "Application terminated unexpectedly");
	// Re-throw to allow the browser to handle the error
	throw;
}
