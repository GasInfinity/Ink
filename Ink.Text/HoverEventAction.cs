namespace Ink.Text;

public enum HoverEventAction
{
    ShowText,
    ShowItem,
    ShowEntity,
}

public static class HoverEventActionExtensions
{
    public static ReadOnlySpan<byte> ShowTextKey => "show_text"u8;
    public static ReadOnlySpan<byte> ShowItemKey => "show_item"u8;
    public static ReadOnlySpan<byte> ShowEntityKey => "show_entity"u8;
    
    public static ReadOnlySpan<byte> ToJsonString(this HoverEventAction kind)
        => kind switch
        {
            HoverEventAction.ShowText => ShowTextKey,
            HoverEventAction.ShowItem => ShowItemKey,
            HoverEventAction.ShowEntity => ShowEntityKey,
            _ => ReadOnlySpan<byte>.Empty
        };

    public static HoverEventAction FromJsonString(ReadOnlySpan<byte> jsonString)
    {
        if(jsonString.SequenceEqual(ShowTextKey))
            return HoverEventAction.ShowText;
        
        if(jsonString.SequenceEqual(ShowItemKey))
            return HoverEventAction.ShowItem;
        
        if(jsonString.SequenceEqual(ShowEntityKey))
            return HoverEventAction.ShowEntity;

        return HoverEventAction.ShowText;
    }
}
