using System.Text.Json.Serialization;
using Shared.Enums;

namespace Shared.Models;

public class AgentTask
{
    public string Id { get; set; } = "";

    [JsonPropertyName("agent_type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AgentTypeEnum AgentType { get; set; }

    public string Prompt { get; set; } = "";

    public List<string> Dependents { get; set; } = [];

    public TaskStateEnum TaskState { get; set; } = TaskStateEnum.Pending;
}