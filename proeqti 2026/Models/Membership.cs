namespace proeqti_2026;

public abstract class Membership : IEntity
{
    public string Id { get; init; }
    public string MemberId { get; init; }
    public DateTime StartDate { get; protected set; }
    public DateTime EndDate { get; protected set; }
    public int? RemainingVisits { get; protected set; }
    public bool IsActive => DateTime.Now.Date <= EndDate.Date && (RemainingVisits is null or > 0);

    protected Membership(string id, string memberId, DateTime start, DateTime end, int? visits)
    {
        Id = id;
        MemberId = memberId;
        StartDate = start;
        EndDate = end;
        RemainingVisits = visits;
    }

    public abstract decimal GetPrice();
    public abstract int? GetVisitLimit();
    public abstract bool CanBook(WorkoutSession session);

    public void Renew(int days, int? visits)
    {
        StartDate = DateTime.Now;
        EndDate = DateTime.Now.AddDays(days);
        RemainingVisits = visits;
    }

    public bool RegisterVisit()
    {
        if (!IsActive) return false;
        if (RemainingVisits.HasValue) RemainingVisits--;
        return true;
    }

    public bool CanEnter() => IsActive;
    public void Expire() => EndDate = DateTime.Now.AddDays(-1);
}

public sealed class BasicMembership : Membership
{
    public BasicMembership(string id, string memberId, DateTime start)
        : base(id, memberId, start, start.AddDays(30), 6) { }

    public override decimal GetPrice() => 50;
    public override int? GetVisitLimit() => 6;
    public override bool CanBook(WorkoutSession session) =>
        session.Type.Equals("Yoga", StringComparison.OrdinalIgnoreCase) ||
        session.Type.Equals("Cardio", StringComparison.OrdinalIgnoreCase);
}

public sealed class PremiumMembership : Membership
{
    public PremiumMembership(string id, string memberId, DateTime start)
        : base(id, memberId, start, start.AddDays(30), null) { }

    public override decimal GetPrice() => 90;
    public override int? GetVisitLimit() => null;
    public override bool CanBook(WorkoutSession session) => true;
}
