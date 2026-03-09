using System.Diagnostics;
using PlannerAgent.Common.Enums;
using PlannerAgent.Common.Models;

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
        try
        {
            var response = await _client.PostAsJsonAsync(Endpoint, task);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"{AgentName} endpoint error: {response.StatusCode}");
            }
            
            var agentResponse = await response.Content.ReadFromJsonAsync<AgentResponse>() ?? throw new Exception($"Failed to deserialize agentResponse at {AgentName} ExecuteAsync call");

            if (!agentResponse.IsSuccess)
            {
                _logger.LogError("{AgentName} returned failed response to PlannerAgent on task: {TaskId} with error: {Error} at {Timestamp}", AgentName, task.Id, agentResponse.Error, DateTime.UtcNow);
            }
            else
            {
                _logger.LogInformation("{AgentName} returned success response to PlannerAgent on task: {TaskId} at {Timestamp}", AgentName, task.Id, DateTime.UtcNow);
            }

            return agentResponse;
        }
        catch (HttpRequestException hx)
        {
            _logger.LogError(hx, "{AgentName} endpoint HTTP error: {error} on task: {taskId} at {DateTime.UtcNow}", AgentName, hx.Message, task.Id, DateTime.UtcNow);  
            return new AgentResponse
            {
                TaskId = task.Id,
                IsSuccess = false,
                Error = hx.Message,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error at {AgentName} ExecuteAsync: {error} on task: {taskId} at {DateTime.UtcNow}", AgentName, ex.Message, task.Id, DateTime.UtcNow);  
            return new AgentResponse
            {
                TaskId = task.Id,
                IsSuccess = false,
                Error = ex.Message,
            };
        }
    }
}