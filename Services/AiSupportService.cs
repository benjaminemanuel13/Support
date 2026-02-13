using System.Text.RegularExpressions;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using Support.Data;

namespace Support.Services;

public interface IAiSupportService
{
    bool IsConfigured { get; }
    Task<int?> IdentifySupportAreaIdAsync(string query, List<SupportArea> areas);
    Task<int?> IdentifySpecificIssueIdAsync(string query, List<SpecificIssue> issues);
}

public class AiSupportService : IAiSupportService
{
    private readonly ChatClient? _chatClient;
    private readonly string _deploymentName;
    private readonly ILogger<AiSupportService> _logger;

    public bool IsConfigured => _chatClient != null;

    public AiSupportService(IConfiguration configuration, ILogger<AiSupportService> logger)
    {
        _logger = logger;
        
        var endpoint = configuration["AZURE_OPENAI_ENDPOINT"] ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        var key = configuration["AZURE_OPENAI_API_KEY"] ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
        _deploymentName = configuration["AZURE_OPENAI_CHAT_DEPLOYMENT_NAME"] ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME") ?? "gpt-4o";

        if (string.IsNullOrEmpty(endpoint)) _logger.LogWarning("AZURE_OPENAI_ENDPOINT is missing.");
        if (string.IsNullOrEmpty(key)) _logger.LogWarning("AZURE_OPENAI_API_KEY is missing.");

        if (!string.IsNullOrEmpty(endpoint) && !string.IsNullOrEmpty(key))
        {
            try 
            {
                var azureClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
                _chatClient = azureClient.GetChatClient(_deploymentName);
                _logger.LogInformation("AiSupportService initialized successfully with deployment: {Deployment}", _deploymentName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize AzureOpenAIClient");
            }
        }
        else
        {
             _logger.LogWarning("Azure OpenAI credentials not found. AI features will be disabled.");
        }
    }

    public async Task<int?> IdentifySupportAreaIdAsync(string query, List<SupportArea> areas)
    {
        if (_chatClient == null) 
        {
            _logger.LogWarning("IdentifySupportAreaIdAsync called but ChatClient is null.");
            return null;
        }

        var areasDescription = string.Join("\n", areas.Select(a => $"ID: {a.Id}, Name: {a.Name}, Description: {a.Description}"));
        
        var prompt = $@"You are a support assistant. 
Here is a list of Support Areas:
{areasDescription}

The user has the following query: '{query}'

Which Support Area ID best matches this query? 
Analyze the Name and Description of each area.
Return ONLY the integer ID of the best match. Do not return any text, explanation, or punctuation. If no area matches well, return -1.";

        _logger.LogInformation("Sending Support Area prompt for query: {Query}", query);
        return await GetIdFromAi(prompt);
    }

    public async Task<int?> IdentifySpecificIssueIdAsync(string query, List<SpecificIssue> issues)
    {
         if (_chatClient == null)
         {
             _logger.LogWarning("IdentifySpecificIssueIdAsync called but ChatClient is null.");
             return null;
         }

        var issuesDescription = string.Join("\n", issues.Select(i => $"ID: {i.Id}, Name: {i.Name}, Description: {i.Description}"));
        
        var prompt = $@"You are a support assistant. 
Here is a list of Specific Issues:
{issuesDescription}

The user has the following query: '{query}'

Which Specific Issue ID best matches this query? 
Analyze the Name and Description of each issue.
Return ONLY the integer ID of the best match. Do not return any text, explanation, or punctuation. If no issue matches well, return -1.";

        _logger.LogInformation("Sending Specific Issue prompt for query: {Query}", query);
        return await GetIdFromAi(prompt);
    }

    private async Task<int?> GetIdFromAi(string prompt)
    {
        try
        {
            ChatCompletion completion = await _chatClient!.CompleteChatAsync(
                new List<ChatMessage> { new UserChatMessage(prompt) }
            );

            var responseText = completion.Content[0].Text.Trim();
            _logger.LogInformation("AI Response: {Response}", responseText);
            
            // Try strict parse first
            if (int.TryParse(responseText, out int id) && id != -1)
            {
                return id;
            }

            // Fallback: Regex to find the first integer
            var match = Regex.Match(responseText, @"\d+");
            if (match.Success && int.TryParse(match.Value, out int regexId) && regexId != -1)
            {
                _logger.LogInformation("Extracted ID via Regex: {Id}", regexId);
                return regexId;
            }

            _logger.LogWarning("Failed to parse AI response as ID.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling AI service");
            return null;
        }
    }
}
