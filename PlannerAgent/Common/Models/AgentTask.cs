using System.Text.Json.Serialization;
using PlannerAgent.Common.Enums;

namespace PlannerAgent.Common.Models;

public class AgentTask (
    string id,
    AgentTypeEnum agentType,
    string prompt,
    List<string> dependents
)
{
    public string Id { get; set; } = id;

    [JsonPropertyName("agent_type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AgentTypeEnum AgentType { get; set; } = agentType;
    public string Prompt { get; set; } = prompt;
    public List<string> Dependents { get; set; } = dependents;
    public TaskStateEnum TaskState { get; set; } = TaskStateEnum.Pending;
}