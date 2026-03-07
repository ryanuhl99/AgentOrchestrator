using Infra;
using System.Net.Http.Headers;
using PlannerAgent.Common.Models;
using System.Net.Http;
using PlannerAgent.Common.Enums;

namespace PlannerAgent.Services.Clients;

public class LlmClient
{
    private readonly HttpClient _client;
    private readonly ILogger<LlmClient> _logger;

    public LlmClient(HttpClient client, IConfiguration config, ILogger<LlmClient> logger)
    {
        _client = client;
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue(
                "Bearer",
                config["OpenAi:ApiKey"]
            );
        _logger = logger;
    }

    public async Task<string> CompleteAsync(string prompt)
    {
        try
        {
            var uri = "/v1/chat/completions";
            using var response = await _client.PostAsJsonAsync(uri, new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<OpenAiErrorResponse>();
                throw new HttpRequestException($"Error at OpenAi Completions endpoint: {error?.Error?.Message}");
            }

            var body = await response.Content.ReadFromJsonAsync<OpenAiLlmResponse>();
            return body!.Choices[0].Message.Content;
        }
        catch (HttpRequestException hx)
        {
            _logger.LogAgentError(
                hx,
                AgentTypeEnum.PlannerAgent.ToString(),
                
            )
        }
        catch (Exception ex)
        {
            
        }
    }
}