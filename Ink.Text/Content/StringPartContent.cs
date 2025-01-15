using System.Text;

namespace Ink.Text.Content;

public sealed record StringPartContent : IPartContent
{
    public static readonly StringPartContent Empty = new(string.Empty);

    public string Content { get; }

    public StringPartContent(string content)
        => Content = content;

    public void AppendPlainText<TProvider>(StringBuilder builder, TProvider provider)
        where TProvider : IContentDataProvider
        => builder.Append(Content);
}
