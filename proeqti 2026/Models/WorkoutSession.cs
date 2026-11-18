namespace proeqti_2026;

public sealed class WorkoutSession : IEntity
{
    public string Id { get; init; }
    public string TrainerId { get; init; }
    public string Type { get; set; }
    public DateTime Start { get; init; }
    public int DurationMinutes { get; init; }
    public int Capacity { get; init; }
    public bool IsCancelled { get; private set; }

    private readonly HashSet<string> memberIds = new();

    public int FreePlaces => Capacity - memberIds.Count;
    public IReadOnlyCollection<string> MemberIds => memberIds;

    public WorkoutSession(string id, string trainerId, string type, DateTime start, int duration, int capacity)
    {
        Id = id;
        TrainerId = trainerId;
        Type = type;
        Start = start;
        DurationMinutes = duration;
        Capacity = capacity;
    }

    public bool AddMember(string memberId) =>
        !IsCancelled && FreePlaces > 0 && memberIds.Add(memberId);

    public bool RemoveMember(string memberId) => memberIds.Remove(memberId);
    public void Cancel() => IsCancelled = true;

    public bool Overlaps(WorkoutSession other) =>
        Start < other.Start.AddMinutes(other.DurationMinutes) &&
        other.Start < Start.AddMinutes(DurationMinutes);
}
