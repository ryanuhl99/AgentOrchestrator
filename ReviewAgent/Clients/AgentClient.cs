using Infra;
using System.Net.Http.Headers;
using System.Net.Http;
using Shared.Enums;
using Shared.Models;
using System.Diagnostics;
using System.Text.Json;

namespace ReviewAgent.Clients;


public class AgentClient
{
    private readonly HttpClient _client;
    private readonly ILogger<AgentClient> _logger;

    public AgentClient(HttpClient client, IConfiguration config, ILogger<AgentClient> logger)
    {
        _client = client;
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue(
                "Bearer",
                config["OpenAi:ApiKey"]
            );
        _logger = logger;
    }

    public async Task<AgentResponse> CompleteAsync(AgentTask task)
    {
        var sw = Stopwatch.StartNew();
        OpenAiErrorResponse? error = null;

        try
        {
            var response = await _client.PostAsJsonAsync("/v1/chat/completions", new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "user", content = task.Prompt }
                }
            });

            if (!response.IsSuccessStatusCode)
            {
                error = await response.Content.ReadFromJsonAsync<OpenAiErrorResponse>();

                throw new HttpRequestException(
                    $"ReviewAgent error at CompleteAsync Status: {response.StatusCode} Error: {error?.Error?.Message}"
                );
            }

            var body = await response.Content.ReadFromJsonAsync<OpenAiLlmResponse>();
            var agentResponseContent = body?.Choices?[0].Message.Content ?? "";
            var usage = body?.Usage;
            var inputToks = usage is null ? 0 : usage.PromptTokens;
            var completionToks = usage is null ? 0 : usage.CompletionTokens;

            if (string.IsNullOrWhiteSpace(agentResponseContent))
            {
                throw new Exception("ReviewAgent CompleteAsync returned empty response");
            }

            agentResponseContent = agentResponseContent
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            sw.Stop();

            _logger.LogAgentInfo(
                task.AgentType.ToString(),
                task.Id,
                inputToks,
                completionToks,
                sw.ElapsedMilliseconds,
                true
            );

            return new AgentResponse
            {
                TaskId = task.Id,
                Output = agentResponseContent,
                IsSuccess = true,
                InputTokens = inputToks,
                OutputTokens = completionToks,
                LatencyMs = sw.ElapsedMilliseconds
            };
        }
        catch (HttpRequestException hx)
        {
            sw.Stop();

            _logger.LogAgentError(
                hx,
                task.AgentType.ToString(),
                task.Id,
                0,
                0,
                sw.ElapsedMilliseconds,
                false
            );

            return new AgentResponse
            {
                TaskId = task.Id,
                IsSuccess = false,
                Error = error?.Error?.Message ?? hx.Message,
                InputTokens = 0,
                OutputTokens = 0,
                LatencyMs = sw.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            sw.Stop();

            _logger.LogAgentError(
                ex,
                task.AgentType.ToString(),
                task.Id,
                0,
                0,
                sw.ElapsedMilliseconds,
                false
            );

            return new AgentResponse
            {
                TaskId = task.Id,
                IsSuccess = false,
                Error = error?.Error?.Message ?? ex.Message,
                InputTokens = 0,
                OutputTokens = 0,
                LatencyMs = sw.ElapsedMilliseconds
            };
        }
    }
}