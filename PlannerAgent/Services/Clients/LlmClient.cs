using Infra;
using System.Net.Http.Headers;
using PlannerAgent.Common.Models;
using System.Net.Http;
using PlannerAgent.Common.Enums;
using System.Diagnostics;
using System.Text.Json;

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

    public async Task<LlmResponseDto> CompleteAsync(string prompt)
    {
        var sw = Stopwatch.StartNew();
        const int maxRetries = 3;
        int retries = 0;
        OpenAiErrorResponse? error = null;

        while (retries <= maxRetries)
        {
            try
            {
                var response = await _client.PostAsJsonAsync("/v1/chat/completions", new
                {
                    model = "gpt-4o-mini",
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    }
                });

                // retryable errors
                if ((int)response.StatusCode == 429 ||
                    (int)response.StatusCode >= 500)
                {
                    throw new HttpRequestException(
                        $"Retryable OpenAI error {(int)response.StatusCode}"
                    );
                }

                // non-retryable API errors
                if (!response.IsSuccessStatusCode)
                {
                    error = await response.Content.ReadFromJsonAsync<OpenAiErrorResponse>();

                    throw new HttpRequestException(
                        $"OpenAI error: {error?.Error?.Message}"
                    );
                }

                var body = await response.Content.ReadFromJsonAsync<OpenAiLlmResponse>();
                var jsonContent = body?.Choices?[0].Message.Content ?? "";

                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    throw new Exception("OpenAi Completions endpoint returned empty response");
                }

                jsonContent = jsonContent
                    .Replace("```json", "")
                    .Replace("```", "")
                    .Trim();

                var graph = JsonSerializer.Deserialize<AgentGraph>(
                    jsonContent,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }
                ) ?? throw new Exception("OpenAi Completions content failed to deserialize into graph object");

                sw.Stop();

                _logger.LogAgentInfo(
                    AgentTypeEnum.PlannerAgent.ToString(),
                    "task_decomposition",
                    body?.Usage?.PromptTokens ?? 0,
                    body?.Usage?.CompletionTokens ?? 0,
                    sw.ElapsedMilliseconds,
                    true
                );

                return new LlmResponseDto
                {
                    Graph = graph,
                    SuccessResponse = body,
                    Latency = sw.ElapsedMilliseconds,
                    UserPrompt = prompt
                };
            }
            catch (HttpRequestException ex) when (retries < maxRetries)
            {
                retries++;

                var delay = TimeSpan.FromSeconds(Math.Pow(2, retries));

                _logger.LogWarning(
                    "Retrying OpenAI call (attempt {Retry}/{MaxRetries}) after {Delay}s: {Message}",
                    retries,
                    maxRetries,
                    delay.TotalSeconds,
                    ex.Message
                );

                await Task.Delay(delay);
            }
            catch (Exception ex)
            {
                sw.Stop();

                _logger.LogAgentError(
                    ex,
                    AgentTypeEnum.PlannerAgent.ToString(),
                    "task_decomposition",
                    0,
                    0,
                    sw.ElapsedMilliseconds,
                    false
                );

                return new LlmResponseDto
                {
                    Graph = new(),
                    FailResponse = error,
                    Latency = sw.ElapsedMilliseconds,
                    UserPrompt = prompt
                };
            }
        }

        // retries exhausted
        sw.Stop();

        var finalEx = new Exception("OpenAI call failed after max retries");

        _logger.LogAgentError(
            finalEx,
            AgentTypeEnum.PlannerAgent.ToString(),
            "task_decomposition",
            0,
            0,
            sw.ElapsedMilliseconds,
            false
        );

        return new LlmResponseDto
        {
            Graph = new(),
            FailResponse = error,
            Latency = sw.ElapsedMilliseconds,
            UserPrompt = prompt
        };
    }
}