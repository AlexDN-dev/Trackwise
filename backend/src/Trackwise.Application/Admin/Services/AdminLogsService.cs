using System.Text.RegularExpressions;
using Microsoft.Extensions.Hosting;
using Trackwise.Application.Admin.Dtos;

namespace Trackwise.Application.Admin.Services;

public partial class AdminLogsService : IAdminLogsService
{
    // Serilog format : "2026-07-28 14:23:01.123 +02:00 [INF] Message..."
    [GeneratedRegex(@"^(?<ts>\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2}\.\d{3}\s[+\-]\d{2}:\d{2})\s\[(?<lvl>[A-Z]{3})\]")]
    private static partial Regex LogLineRegex();

    private readonly IHostEnvironment _env;

    public AdminLogsService(IHostEnvironment env) => _env = env;

    public async Task<LogsResponse> ReadRecentAsync(
        int lines, string? levelFilter, string? searchFilter, CancellationToken cancellationToken = default)
    {
        var logsDir = Path.Combine(_env.ContentRootPath, "logs");
        if (!Directory.Exists(logsDir))
            return new LogsResponse();

        var latestFile = new DirectoryInfo(logsDir)
            .EnumerateFiles("trackwise-*.log")
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .FirstOrDefault();

        if (latestFile is null)
            return new LogsResponse();

        var allLines = new List<string>();
        using (var stream = new FileStream(latestFile.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var reader = new StreamReader(stream))
        {
            string? line;
            while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
            {
                allLines.Add(line);
            }
        }

        var totalLines = allLines.Count;

        IEnumerable<string> filtered = allLines;

        if (!string.IsNullOrWhiteSpace(levelFilter))
        {
            var lvl = levelFilter.ToUpperInvariant();
            filtered = filtered.Where(l => l.Contains($"[{lvl}]"));
        }

        if (!string.IsNullOrWhiteSpace(searchFilter))
        {
            var term = searchFilter.Trim();
            filtered = filtered.Where(l => l.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        var tail = filtered.TakeLast(Math.Clamp(lines, 1, 1000)).ToList();

        return new LogsResponse
        {
            FileName = latestFile.Name,
            TotalLinesInFile = totalLines,
            Entries = tail.Select(ParseLine).ToList(),
        };
    }

    private static LogEntry ParseLine(string line)
    {
        var match = LogLineRegex().Match(line);
        if (!match.Success)
            return new LogEntry { RawLine = line };

        DateTimeOffset.TryParse(match.Groups["ts"].Value, out var ts);
        return new LogEntry
        {
            RawLine = line,
            Level = match.Groups["lvl"].Value,
            Timestamp = ts,
        };
    }
}