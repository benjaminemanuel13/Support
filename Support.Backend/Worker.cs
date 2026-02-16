using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Playwright;
using Support.Common.Models;

namespace Support.Backend;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public TcpListener Listener { get; private set; }

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
        Listener = new TcpListener(IPAddress.Any, 13000);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Listener.Start();
        _logger.LogInformation("TCP Listener started on port 13000.");

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            if (Listener.Pending())
            {
                try
                {
                    using var client = await Listener.AcceptTcpClientAsync(stoppingToken);
                    _logger.LogInformation("Client connected!");
                    
                    using var stream = client.GetStream();
                    using var reader = new StreamReader(stream);
                    

                    string? data = await reader.ReadLineAsync(stoppingToken);
                    if (data != null)
                    {
                        await ProcessReceivedData(data);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error accepting client");
                }
            }

            await Task.Delay(1000, stoppingToken);
        }

        Listener.Stop();
    }

    private async Task ProcessReceivedData(string data)
    {
        try 
        {
            var request = JsonSerializer.Deserialize<PlayWrightRequest>(data);
            if (request != null && !string.IsNullOrEmpty(request.Url) && request.WebTasks != null)
            {
                using var playwright = await Playwright.CreateAsync();
                await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false, SlowMo = 1000 });
                var page = await browser.NewPageAsync();
                await page.GotoAsync(request.Url);
                await page.BringToFrontAsync();

                foreach (var task in request.WebTasks)
                {
                    if (!string.IsNullOrEmpty(task.SpeechText))
                    {
                        await SpeakAsync(task.SpeechText);
                        await Task.Delay(2000);
                    }

                    if (!string.IsNullOrEmpty(task.ElementId))
                    {
                        await page.Locator($"#{task.ElementId}").ClickAsync();
                        // Wait for potential navigation or network idle after click
                        try 
                        {
                            await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 5000 });
                        }
                        catch (TimeoutException) 
                        {
                            // Continue if no navigation occurs within timeout
                        }
                    }
                }
                
                // Keep browser open for a moment to see final state? User didn't specify closing.
                // But for a worker, we should probably close it to avoid resource leaks.
                // Assuming "continue loop" means proceed to next task.
                // I'll close it at the end of the request processing.
                // I'll close it at the end of the request processing.
                await Task.Delay(10000); // 10 second delay to see final result
                await browser.CloseAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing data");
        }
    }

    public async Task SpeakAsync(string text)
    {
        try
        {
            var speechKey = Environment.GetEnvironmentVariable("AZURESPEECHKEY");
            var speechEndpoint = Environment.GetEnvironmentVariable("AZURESPEECHENDPOINT");

            if (string.IsNullOrEmpty(speechKey) || string.IsNullOrEmpty(speechEndpoint))
            {
                _logger.LogWarning("Azure Speech credentials not found in environment variables.");
                return;
            }

            // Assuming AZURESPEECHENDPOINT is a valid URI
            var speechConfig = SpeechConfig.FromEndpoint(new Uri(speechEndpoint), speechKey);
            speechConfig.SpeechSynthesisVoiceName = "en-US-AvaMultilingualNeural"; 

            using var synthesizer = new SpeechSynthesizer(speechConfig);
            
            _logger.LogInformation("Synthesizing speech for: {Text}", text);
            var result = await synthesizer.SpeakTextAsync(text);

            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                _logger.LogInformation("Speech synthesis completed.");
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                _logger.LogWarning("Speech synthesis canceled: {Reason}, ErrorDetails: {Error}", cancellation.Reason, cancellation.ErrorDetails);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during speech synthesis");
        }
    }
}
