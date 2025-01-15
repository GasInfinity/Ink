namespace Ink.Server.Event;

public interface IAsyncPreLoginListener
{
    bool OnPreLogin(PreLoginEvent preloginEvent);
}
