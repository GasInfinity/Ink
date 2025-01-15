namespace Ink.Server.Event;

public interface ILoginListener
{
    void OnLogin(ref LoginEvent loginEvent);
}
