using System.Text;

namespace Ink.Text.Content;

public sealed record KeyPartContent : IPartContent
{
    public string Keybind { get; }

    public KeyPartContent(string keybind)
        => Keybind = keybind;

    public void AppendPlainText<TProvider>(StringBuilder builder, TProvider provider)
        where TProvider : IContentDataProvider
        => provider.AppendKeyname(builder, Keybind);
}
