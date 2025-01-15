namespace Ink.Event.Registry;

public interface IValueRegistrationListener<TValue>
    where TValue : class
{
    void OnRegistration(ValueRegistrationEvent<TValue> registrationEvent);
}
