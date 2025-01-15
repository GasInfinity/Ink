namespace Ink.Resources;

public enum ResourcePackResult : int
{
    SuccessfullyDownloaded = 0,
    Declined,
    FailedToDownload,
    Accepted,
    Downloaded,
    InvalidUrl,
    FailedToReload,
    Discarded
}
