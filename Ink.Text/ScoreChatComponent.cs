using System.Text.Json.Serialization;

namespace Ink.Text;

[method: JsonConstructor]
public record struct ScoreChatComponent(string Name, string Objective, string Value)
{
    public readonly string Name = Name;
    public readonly string Objective = Objective;
    public readonly string Value = Value;
}
