using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Serilog.Core;
using Serilog.Events;

namespace Trackwise.API.Logging;

public sealed partial class RedactionEnricher : ILogEventEnricher
{
    [GeneratedRegex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"eyJ[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+")]
    private static partial Regex JwtRegex();
    
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        foreach (var (name, value) in logEvent.Properties.ToList())
        {
            if (value is ScalarValue { Value: string s })
            {
                var redacted = Redact(s);
                if (!ReferenceEquals(redacted, s))
                {
                    logEvent.AddOrUpdateProperty(
                        new LogEventProperty(name, new ScalarValue(redacted)));
                }
            }
        }
    }

    private static string Redact(string input)
    {
        var result = JwtRegex().Replace(input, "[REDACTED_JWT]");
        result = EmailRegex().Replace(result, m => $"email:sha256:{HashEmail(m.Value)}");
        return result;
    }

    private static string HashEmail(string email)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(email.ToLowerInvariant()));
        return Convert.ToHexString(bytes)[..8].ToLowerInvariant();
    }
}