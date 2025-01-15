using System.Text;

namespace Ink.Text.Content;

public sealed record ScorePartContent : IPartContent
{
    public ScoreChatComponent Score { get; init; }

    public ScorePartContent(in ScoreChatComponent score)
        => Score = score;

    public void AppendPlainText<TProvider>(StringBuilder builder, TProvider provider)
        where TProvider : IContentDataProvider
        => builder.Append($"{Score}");
}
