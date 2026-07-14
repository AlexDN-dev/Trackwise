namespace Trackwise.Domain.Enums;

public enum CandidatureStatus : byte
{
    ToSend = 1,
    Sent = 2,
    PositiveResponse = 3,
    NegativeResponse = 4,
    GotInterview = 5
}