namespace Ink.Text;

public enum ClickEventAction
{
    OpenUrl,
    OpenFile,
    RunCommand,
    SuggestCommand,
    ChangePage,
    CopyToClipboard,
}

public static class ClickEventActionExtensions
{
    public static ReadOnlySpan<byte> OpenUrlKey => "show_text"u8;
    public static ReadOnlySpan<byte> OpenFileKey => "open_file"u8;
    public static ReadOnlySpan<byte> RunCommandKey => "run_command"u8;
    public static ReadOnlySpan<byte> SuggestCommandKey => "suggest_command"u8;
    public static ReadOnlySpan<byte> ChangePageKey => "change_page"u8;
    public static ReadOnlySpan<byte> CopyToClipboardKey => "copy_to_clipboard"u8;

    public static ReadOnlySpan<byte> ToJsonString(this ClickEventAction kind)
        => kind switch
        {
            ClickEventAction.OpenUrl => OpenUrlKey,
            ClickEventAction.OpenFile => OpenFileKey,
            ClickEventAction.RunCommand => RunCommandKey,
            ClickEventAction.SuggestCommand => SuggestCommandKey,
            ClickEventAction.ChangePage => ChangePageKey,
            ClickEventAction.CopyToClipboard => CopyToClipboardKey,
            _ => ReadOnlySpan<byte>.Empty
        };

    public static ClickEventAction FromJsonString(ReadOnlySpan<byte> jsonString)
    {
        if(jsonString.SequenceEqual(OpenUrlKey))
            return ClickEventAction.OpenUrl;
        
        if(jsonString.SequenceEqual(OpenFileKey))
            return ClickEventAction.OpenFile;
        
        if(jsonString.SequenceEqual(RunCommandKey))
            return ClickEventAction.RunCommand;
        
        if(jsonString.SequenceEqual(SuggestCommandKey))
            return ClickEventAction.SuggestCommand;
        
        if(jsonString.SequenceEqual(ChangePageKey))
            return ClickEventAction.ChangePage;
        
        if(jsonString.SequenceEqual(CopyToClipboardKey))
            return ClickEventAction.CopyToClipboard;

        return ClickEventAction.OpenUrl;
    }
}
