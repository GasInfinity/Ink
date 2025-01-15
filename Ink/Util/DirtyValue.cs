namespace Ink.Util;

public record struct DirtyValue<T>
{
    private T value;
    private bool isDirty;

    public T Value
    {
        readonly get => value;
        set
        {
            if (EqualityComparer<T>.Default.Equals(this.value, value))
                return;

            this.value = value;
            this.isDirty = true;
        }
    }

    public readonly bool IsDirty
        => this.isDirty;

    public DirtyValue(T defaultValue)
        => this.value = defaultValue;

    public void ClearDirty()
        => this.isDirty = false;
}
