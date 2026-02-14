using System.Net;
using System.Net.Sockets;
using Microsoft.CognitiveServices.Speech;

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
                        ProcessReceivedData(data);
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

    private void ProcessReceivedData(string data)
    {
        // Placeholder for processing logic
        _ = SpeakAsync($"Received data: {data}");
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
