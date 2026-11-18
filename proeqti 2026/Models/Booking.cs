namespace proeqti_2026;

public sealed class Booking : IEntity
{
    public string Id { get; init; }
    public string MemberId { get; init; }
    public string SessionId { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.Now;
    public bool IsCancelled { get; private set; }

    public Booking(string id, string memberId, string sessionId)
    {
        Id = id;
        MemberId = memberId;
        SessionId = sessionId;
    }

    public void Cancel() => IsCancelled = true;
}

public sealed class Visit : IEntity
{
    public string Id { get; init; }
    public string MemberId { get; init; }
    public DateTime Date { get; init; } = DateTime.Now;

    public Visit(string id, string memberId)
    {
        Id = id;
        MemberId = memberId;
    }
}
