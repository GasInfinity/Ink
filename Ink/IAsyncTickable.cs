namespace Ink;

public interface IAsyncTickable
{
    ValueTask TickAsync();
}
