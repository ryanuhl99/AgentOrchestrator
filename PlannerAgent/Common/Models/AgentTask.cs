using Common.Enums;

namespace Common.Models;

public class AgentTask (
    string id,
    AgentTypeEnum agentType,
    string prompt,
    List<string> dependents,
    TaskStateEnum taskState
)
{
    public string Id { get; set; } = id;
    public AgentTypeEnum AgentType { get; set; } = agentType;
    public string Prompt { get; set; } = prompt;
    public List<string> Dependents { get; set; } = dependents;
    public TaskStateEnum TaskState { get; set; } = taskState;
}