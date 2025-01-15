using System.Collections.Immutable;
using System.Text;

namespace Ink.Text;

public interface IContentDataProvider
{
    public static readonly IContentDataProvider Null = new NullProvider();

    void AppendFinalString(StringBuilder builder, string translation, ImmutableArray<TextPart> with);
    void AppendKeyname(StringBuilder builder, string keybind);

    private record NullProvider : IContentDataProvider
    {
        public void AppendFinalString(StringBuilder builder, string translation, ImmutableArray<TextPart> with)
            => builder.Append(translation);

        public void AppendKeyname(StringBuilder builder, string keybind)
            => builder.Append(keybind);
    }
}
