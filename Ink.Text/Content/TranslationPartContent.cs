using System.Collections.Immutable;
using System.Text;

namespace Ink.Text.Content;

public sealed record TranslationPartContent : IPartContent
{
    public string Translate { get; init; }
    public ImmutableArray<TextPart> With { get; init; }

    public TranslationPartContent(string translate, ImmutableArray<TextPart> with)
        => (Translate, With) = (translate, with);

    public void AppendPlainText<TProvider>(StringBuilder builder, TProvider provider)
        where TProvider : IContentDataProvider
        => provider.AppendFinalString(builder, Translate, With);
}
