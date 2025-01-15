using Ink.Items;

namespace Ink.Containers;

public class Container(int size)
{
    private readonly ItemStack[] contents = new ItemStack[size];

    public ItemStack this[int index] { get => this.contents[index]; set => this.contents[index] = value; }

    public int Size
        => this.contents.Length;
}