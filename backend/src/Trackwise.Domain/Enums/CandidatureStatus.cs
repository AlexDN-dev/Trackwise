namespace Trackwise.Domain.Enums;

public enum CandidatureStatus : byte
{
    ToSend = 1,
    Sent = 2,
    GotInterview = 3,
    PositiveResponse = 4,
    NegativeResponse = 5
}
