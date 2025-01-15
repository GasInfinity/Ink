using System.Collections.Immutable;
using Ink.Text;

namespace Ink.Chat;

public sealed record ChatType
{
    public static readonly ChatType Default = new()
    {
        Chat = new()
        {
            TranslationKey = "chat.type.text",
            Parameters = ["sender", "content"]
        },
        Narration = new()
        {
            TranslationKey = "chat.type.text.narrate",
            Parameters = ["sender", "content"]
        }
    };

    public Content Chat { get; init; }
    public Content Narration { get; init; }

    public readonly record struct Content
    {
        public string TranslationKey { get; init; }
        public TextStyle Style { get; init; }
        public ImmutableArray<string> Parameters { get; init; }
    }
}
