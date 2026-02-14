namespace Support.Common.Models;

public class PlayWrightRequest
{
    public int Id { get; set; }
    public required string Url { get; set; }
    public List<WebTask> WebTasks { get; set; } = new();
}
