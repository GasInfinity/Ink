using System.Text;

namespace Ink.Text;

public interface IPartContent
{
    void AppendPlainText<TProvider>(StringBuilder builder, TProvider provider)
        where TProvider : IContentDataProvider;
}
