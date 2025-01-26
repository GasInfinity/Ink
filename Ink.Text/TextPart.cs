using System.Collections.Immutable;
using System.Text;
using Ink.Text.Content;

namespace Ink.Text;

public record struct TextPart(IPartContent? Content = null, TextStyle? Style = null, ImmutableArray<TextPart> Extra = default) : IHoverEventContent
{
    public readonly IPartContent Content = Content ?? StringPartContent.Empty;
    public readonly TextStyle Style = Style ?? TextStyle.Empty;
    public readonly ImmutableArray<TextPart> Extra = Extra;

    public static TextPart Empty()
        => new(StringPartContent.Empty);

    public static TextPart String(string content)
        => new(new StringPartContent(content));

    public void AppendPlainText<TProvider>(StringBuilder builder, TProvider provider)
        where TProvider : IContentDataProvider
    {
        Content.AppendPlainText(builder, provider);

        if(!Extra.IsDefaultOrEmpty)
        {
            foreach(var part in Extra)
            {
                part.AppendPlainText(builder, provider);
            }
        }
    }

    public string ToPlainText<TProvider>(TProvider provider)
        where TProvider : IContentDataProvider
    {
        StringBuilder builder = new();
        AppendPlainText(builder, provider);
        return builder.ToString();
    }

    public override string ToString()
        => ToPlainText(IContentDataProvider.Null);
}
