using System.Text;

namespace Ink.Text.Content;

public sealed record SelectorPartContent : IPartContent
{
    public string Selector { get; }

    public SelectorPartContent(string selector)
        => Selector = selector;

    public void AppendPlainText<TProvider>(StringBuilder builder, TProvider provider) where TProvider : IContentDataProvider
        => builder.Append(Selector);
}
