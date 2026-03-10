using System.Diagnostics;
using Shared.Enums;
using Shared.Models;
using Infra;

namespace PlannerAgent.Services.Clients.Agents;

public abstract class AgentClientBase : IAgent
{
    protected abstract string AgentName { get; }
    protected abstract string Endpoint { get; }

    private readonly HttpClient _client;
    private readonly ILogger _logger;

    protected AgentClientBase(HttpClient client, ILogger logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<AgentResponse> ExecuteAsync(AgentTask task)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            var response = await _client.PostAsJsonAsync(Endpoint, task);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"{AgentName} endpoint error: {response.StatusCode}");
            }

            var agentResponse =
                await response.Content.ReadFromJsonAsync<AgentResponse>()
                ?? throw new Exception($"Failed to deserialize AgentResponse at {AgentName} ExecuteAsync");

            sw.Stop();

            if (!agentResponse.IsSuccess)
            {
                _logger.LogAgentError(
                    new Exception(agentResponse.Error ?? "Agent returned failure"),
                    AgentName,
                    task.Id,
                    agentResponse.InputTokens,
                    agentResponse.OutputTokens,
                    sw.ElapsedMilliseconds,
                    false
                );
            }
            else
            {
                _logger.LogAgentInfo(
                    AgentName,
                    task.Id,
                    agentResponse.InputTokens,
                    agentResponse.OutputTokens,
                    sw.ElapsedMilliseconds,
                    true
                );
            }

            return agentResponse;
        }
        catch (HttpRequestException hx)
        {
            sw.Stop();

            _logger.LogAgentError(
                hx,
                AgentName,
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
                Error = hx.Message
            };
        }
        catch (Exception ex)
        {
            sw.Stop();

            _logger.LogAgentError(
                ex,
                AgentName,
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
                Error = ex.Message
            };
        }
    }
}