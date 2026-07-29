namespace Trackwise.Application.Admin.Dtos;

public class LogEntry
{
    public string RawLine { get; set; } = string.Empty;
    public string? Level { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
}

public class LogsResponse
{
    public List<LogEntry> Entries { get; set; } = new();
    public string FileName { get; set; } = string.Empty;
    public int TotalLinesInFile { get; set; }
}