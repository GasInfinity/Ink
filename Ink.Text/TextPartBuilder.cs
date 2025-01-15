using System.Collections.Immutable;
using Ink.Text.Content;

namespace Ink.Text;

public sealed class TextPartBuilder(IPartContent? Content = null, TextStyle? Style = null, int ExtraCount = default)
{
    private IPartContent content = Content ?? StringPartContent.Empty;
    private TextStyle style = Style ?? TextStyle.Empty;
    private ImmutableArray<TextPart>.Builder extra = ImmutableArray.CreateBuilder<TextPart>(ExtraCount);

    public IPartContent CurrentContent
        => this.content;

    public TextStyle CurrentStyle
        => this.style;

    public TextPartBuilder Content(IPartContent value)
    {
        this.content = value;
        return this;
    }

    public TextPartBuilder Content(string value)
    {
        this.content = new StringPartContent(value);
        return this;
    }

    public TextPartBuilder Style(TextStyle style)
    {
        this.style = style;
        return this;
    }

    public TextPartBuilder Append(params ReadOnlySpan<TextPart> value)
    {
        this.extra.AddRange(value);
        return this;
    }

    public TextPartBuilder CopyFrom(TextPart part)
    {
        this.content = part.Content;
        this.style = part.Style;
        this.extra.Clear();

        if(!part.Extra.IsDefaultOrEmpty)
        {
            this.extra.AddRange(part.Extra);
        }

        return this;
    }

    public TextPart ToTextPart()
        => new (content, style, extra.DrainToImmutable());

    public static TextPartBuilder String(string value, TextStyle? style = null, params ReadOnlySpan<TextPart> extra)
        => new TextPartBuilder(new StringPartContent(value), style, extra.Length).Append(extra);
}
