namespace proeqti_2026;

public abstract class Person : IEntity
{
    public string Id { get; init; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }

    protected Person(string id, string name, string email, string password)
    {
        Id = id;
        FullName = name;
        Email = email;
        PasswordHash = Security.Hash(password);
    }

    public bool CheckPassword(string password) => PasswordHash == Security.Hash(password);
}

public sealed class Member : Person
{
    public Membership? Membership { get; set; }

    public Member(string id, string name, string email, string password)
        : base(id, name, email, password) { }
}

public sealed class Trainer : Person
{
    public string Specialization { get; set; }

    public Trainer(string id, string name, string email, string specialization, string password = "trainer")
        : base(id, name, email, password)
    {
        Specialization = specialization;
    }
}

public sealed class Manager : Person
{
    public Manager(string id, string name, string email, string password)
        : base(id, name, email, password) { }
}

public sealed class Admin : Person
{
    public Admin(string id, string name, string email, string password)
        : base(id, name, email, password) { }
}
