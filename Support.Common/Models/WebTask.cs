namespace Support.Common.Models;

public class WebTask
{
    public int Id { get; set; }
    public string? ElementId { get; set; }
    public WebTaskType ActionType { get; set; }
    public string? Data { get; set; }
    public string? SpeechText { get; set; }

    public int PlayWrightRequestId { get; set; }
    public PlayWrightRequest? PlayWrightRequest { get; set; }
}
