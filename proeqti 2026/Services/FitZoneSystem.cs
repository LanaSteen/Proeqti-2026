namespace proeqti_2026;

public sealed class FitZoneSystem
{
    private readonly FileManager files;
    private readonly Logger logger;
    private readonly IEmailService email;
    private readonly IRepository<Member> members = new Repository<Member>();
    private readonly IRepository<Trainer> trainers = new Repository<Trainer>();
    private readonly IRepository<Admin> admins = new Repository<Admin>();
    private readonly IRepository<WorkoutSession> sessions = new Repository<WorkoutSession>();
    private readonly IRepository<Booking> bookings = new Repository<Booking>();
    private readonly IRepository<Visit> visits = new Repository<Visit>();
    private Member? current;
    private Admin? currentAdmin;

    public FitZoneSystem(FileManager files, Logger logger, IEmailService email)
    {
        this.files = files;
        this.logger = logger;
        this.email = email;
    }

    public void Load() { }

    public void Seed()
    {
        if (members.Items.Count > 0) return;

        admins.Add(new Admin("A001", "Admin User", "admin@fitzone.ge", "admin123"));
        trainers.Add(new Trainer("T01", "Giorgi Maisuradze", "giorgi@fitzone.ge", "Strength"));
        trainers.Add(new Trainer("T02", "Nino Beridze", "nino@fitzone.ge", "Yoga"));
        sessions.Add(new WorkoutSession("WS501", "T02", "Yoga", DateTime.Today.AddDays(1).AddHours(18), 60, 12));
        sessions.Add(new WorkoutSession("WS502", "T01", "Strength", DateTime.Today.AddDays(2).AddHours(19), 60, 8));
    }

    public void Register()
    {
        var id = $"M{members.Items.Count + 1001}";
        var name = Input.Read("სახელი და გვარი: ");
        var emailAddress = Input.Read("ელფოსტა: ");
        var password = Input.Read("პაროლი: ");

        if (members.Items.Any(x => x.Email.Equals(emailAddress, StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine("ეს ელფოსტა უკვე რეგისტრირებულია.");
            Input.Pause();
            return;
        }

        members.Add(new Member(id, name, emailAddress, password));
        logger.Write($"REGISTER member={id}");
        Console.WriteLine($"რეგისტრაცია წარმატებულია. თქვენი ID: {id}");
        Input.Pause();
    }

    public void Login()
    {
        var id = Input.Read("Member/Admin ID: ");

        // Check if Admin
        var admin = admins.Find(id);
        if (admin is not null && admin.CheckPassword(Input.Read("პაროლი: ")))
        {
            currentAdmin = admin;
            AdminMenu();
            currentAdmin = null;
            return;
        }

        // Check if Member
        var member = members.Find(id);
        if (member is null || !member.CheckPassword(Input.Read("პაროლი: ")))
        {
            Console.WriteLine("არასწორი მონაცემები.");
            Input.Pause();
            return;
        }

        current = member;
        MemberMenu();
        current = null;
    }

    private void MemberMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"=== წევრი: {current!.FullName} ===");
            Console.WriteLine("1. აბონემენტი\n2. განრიგი\n3. ჩაწერა\n4. ჩემი ჩანაწერები\n5. ჩანაწერის გაუქმება\n6. ვიზიტი\n0. გამოსვლა");

            switch (Input.Read("არჩევანი: "))
            {
                case "1": MembershipMenu(); break;
                case "2": ShowSessions(); break;
                case "3": Book(); break;
                case "4": ShowBookings(); break;
                case "5": CancelBooking(); break;
                case "6": Visit(); break;
                case "0": return;
            }
        }
    }

    private void MembershipMenu()
    {
        if (current!.Membership is not null)
        {
            Console.WriteLine($"{current.Membership.GetType().Name}: {current.Membership.GetPrice():C}, " +
                $"ვადა {current.Membership.EndDate:d}, ვიზიტები " +
                $"{current.Membership.RemainingVisits?.ToString() ?? "ულიმიტო"}");
        }

        Console.WriteLine("\n1. Basic (50, 6 ვიზიტი)\n2. Premium (90, ულიმიტო)\n0. უკან");
        var choice = Input.Read("არჩევანი: ");
        if (choice is not ("1" or "2")) return;

        current.Membership = choice == "1"
            ? new BasicMembership($"MS{DateTime.Now:MMddHHmmss}", current.Id, DateTime.Now)
            : new PremiumMembership($"MS{DateTime.Now:MMddHHmmss}", current.Id, DateTime.Now);

        logger.Write($"MEMBERSHIP member={current.Id}");
        Console.WriteLine("აბონემენტი გააქტიურდა.");
        Input.Pause();
    }

    private void ShowSessions()
    {
        var type = Input.Read("ვარჯიშის ტიპი (Enter=ყველა): ");
        var result = sessions.Items
            .Where(s => !s.IsCancelled && s.Start > DateTime.Now &&
                (string.IsNullOrWhiteSpace(type) || s.Type.Contains(type, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        foreach (var session in result)
        {
            var trainer = trainers.Find(session.TrainerId);
            Console.WriteLine($"{session.Id} | {session.Type} | {session.Start:g} | " +
                $"{session.DurationMinutes} წთ | {trainer?.FullName} | " +
                $"თავისუფალი: {session.FreePlaces}/{session.Capacity}");
        }

        if (result.Count == 0) Console.WriteLine("სესიები ვერ მოიძებნა.");
        Input.Pause();
    }

    private void Book()
    {
        if (current!.Membership is null || !current.Membership.IsActive)
        {
            Console.WriteLine("აქტიური აბონემენტი აუცილებელია.");
            Input.Pause();
            return;
        }

        ShowSessions();
        var session = sessions.Find(Input.Read("Session ID: "));
        if (session is null || !current.Membership.CanBook(session) || !session.AddMember(current.Id))
        {
            Console.WriteLine("ჩაწერა ვერ შესრულდა (ტევადობა, ტიპი ან დუბლირება).");
            Input.Pause();
            return;
        }

        if (bookings.Items.Any(b => b.MemberId == current.Id && !b.IsCancelled &&
            sessions.Find(b.SessionId)!.Overlaps(session)))
        {
            session.RemoveMember(current.Id);
            Console.WriteLine("დროის გადაფარვა დაუშვებელია.");
            Input.Pause();
            return;
        }

        bookings.Add(new Booking($"B{bookings.Items.Count + 801}", current.Id, session.Id));
        logger.Write($"BOOK member={current.Id} session={session.Id}");
        Console.WriteLine("ჩაწერა წარმატებულია.");
        Input.Pause();
    }

    private void ShowBookings()
    {
        foreach (var booking in bookings.Items.Where(x => x.MemberId == current!.Id && !x.IsCancelled))
            Console.WriteLine($"{booking.Id} | {booking.SessionId} | {sessions.Find(booking.SessionId)?.Start:g}");

        Input.Pause();
    }

    private void CancelBooking()
    {
        var booking = bookings.Find(Input.Read("Booking ID: "));
        var session = booking is null ? null : sessions.Find(booking.SessionId);

        if (booking is null || booking.MemberId != current!.Id || booking.IsCancelled ||
            session is null || session.Start <= DateTime.Now)
        {
            Console.WriteLine("გაუქმება შეუძლებელია.");
            Input.Pause();
            return;
        }

        booking.Cancel();
        session.RemoveMember(current.Id);
        logger.Write($"CANCEL booking={booking.Id}");
        Console.WriteLine("ჩანაწერი გაუქმდა.");
        Input.Pause();
    }

    private void Visit()
    {
        if (current!.Membership?.RegisterVisit() != true)
        {
            Console.WriteLine("ვიზიტი ვერ დარეგისტრირდა: აბონემენტი ვადაგასულია ან ვიზიტები ამოიწურა.");
        }
        else
        {
            visits.Add(new Visit($"V{visits.Items.Count + 1}", current.Id));
            logger.Write($"VISIT member={current.Id}");
            Console.WriteLine("ვიზიტი დარეგისტრირდა.");
        }

        Input.Pause();
    }

    private void AdminMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"=== ადმინი: {currentAdmin!.FullName} ===");
            Console.WriteLine("1. ვარჯიშის სესიის დამატება\n2. ვარჯიშის სესიების ჩვენება\n3. ტრენერის დამატება\n4. ტრენერის რედაქტირება\n5. ტრენერის წაშლა\n6. ყველა ტრენერის ჩვენება\n0. გამოსვლა");

            switch (Input.Read("არჩევანი: "))
            {
                case "1": AddWorkoutSession(); break;
                case "2": ShowAllSessions(); break;
                case "3": AddTrainer(); break;
                case "4": UpdateTrainer(); break;
                case "5": DeleteTrainer(); break;
                case "6": GetAllTrainers(); break;
                case "0": return;
            }
        }
    }

    private void AddWorkoutSession()
    {
        var id = $"WS{sessions.Items.Count + 501}";
        var trainerId = Input.Read("ტრენერის ID: ");

        if (trainers.Find(trainerId) is null)
        {
            Console.WriteLine("ტრენერი ვერ მოიძებნა.");
            Input.Pause();
            return;
        }

        var type = Input.Read("ვარჯიშის ტიპი: ");
        Console.WriteLine("თარიღი (YYYY-MM-DD HH:mm): ");
        if (!DateTime.TryParse(Input.Read(""), out var startTime))
        {
            Console.WriteLine("არასწორი თარიღი.");
            Input.Pause();
            return;
        }

        if (!int.TryParse(Input.Read("ხანგრძლივობა (წუთებში): "), out var duration) || duration <= 0)
        {
            Console.WriteLine("არასწორი ხანგრძლივობა.");
            Input.Pause();
            return;
        }

        if (!int.TryParse(Input.Read("სიმძლავრე (ადამიანების რაოდენობა): "), out var capacity) || capacity <= 0)
        {
            Console.WriteLine("არასწორი სიმძლავრე.");
            Input.Pause();
            return;
        }

        // Check for trainer conflicts
        foreach (var session in sessions.Items)
        {
            if (session.TrainerId == trainerId && !session.IsCancelled)
            {
                var existingSession = new WorkoutSession(session.Id, session.TrainerId, session.Type, session.Start, session.DurationMinutes, session.Capacity);
                if (existingSession.Overlaps(new WorkoutSession(id, trainerId, type, startTime, duration, capacity)))
                {
                    Console.WriteLine("ტრენერი ამ დროს მუშაობს.");
                    Input.Pause();
                    return;
                }
            }
        }

        sessions.Add(new WorkoutSession(id, trainerId, type, startTime, duration, capacity));
        logger.Write($"ADMIN_CREATE_SESSION admin={currentAdmin!.Id} session={id}");
        Console.WriteLine($"ვარჯიშის სესია წარმატებით შეიქმნა. ID: {id}");
        Input.Pause();
    }

    private void ShowAllSessions()
    {
        Console.WriteLine("=== ყველა ვარჯიშის სესია ===");
        foreach (var session in sessions.Items)
        {
            var trainer = trainers.Find(session.TrainerId);
            var status = session.IsCancelled ? "გაუქმებული" : "აქტიური";
            Console.WriteLine($"{session.Id} | {session.Type} | {session.Start:g} | " +
                $"{session.DurationMinutes} წთ | {trainer?.FullName} | " +
                $"თავისუფალი: {session.FreePlaces}/{session.Capacity} | {status}");
        }

        Input.Pause();
    }

    private void AddTrainer()
    {
        var id = $"T{trainers.Items.Count:D2}";
        var name = Input.Read("ტრენერის სახელი და გვარი: ");
        var email = Input.Read("ელფოსტა: ");
        var specialization = Input.Read("სპეციალიზაცია: ");

        if (trainers.Items.Any(t => t.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine("ეს ელფოსტა უკვე გამოყენებულია.");
            Input.Pause();
            return;
        }

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(specialization))
        {
            Console.WriteLine("ყველა ველი სავალდებულოა.");
            Input.Pause();
            return;
        }

        trainers.Add(new Trainer(id, name, email, specialization));
        logger.Write($"ADMIN_ADD_TRAINER admin={currentAdmin!.Id} trainer={id}");
        Console.WriteLine($"ტრენერი წარმატებით დამატდა. ID: {id}, პაროლი: trainer");
        Input.Pause();
    }

    private void UpdateTrainer()
    {
        GetAllTrainers();
        var trainerId = Input.Read("ტრენერის ID რომელიც სურთ რედაქტირება: ");
        var trainer = trainers.Find(trainerId);

        if (trainer is null)
        {
            Console.WriteLine("ტრენერი ვერ მოიძებნა.");
            Input.Pause();
            return;
        }

        Console.WriteLine($"მიმდინარე მონაცემები: {trainer.FullName} ({trainer.Specialization})");
        var newName = Input.Read("ახალი სახელი (Enter=უცვლელი): ");
        if (!string.IsNullOrWhiteSpace(newName))
            trainer.FullName = newName;

        var newEmail = Input.Read("ახალი ელფოსტა (Enter=უცვლელი): ");
        if (!string.IsNullOrWhiteSpace(newEmail) && !trainers.Items.Any(t => t.Id != trainer.Id && t.Email.Equals(newEmail, StringComparison.OrdinalIgnoreCase)))
            trainer.Email = newEmail;

        var newSpec = Input.Read("ახალი სპეციალიზაცია (Enter=უცვლელი): ");
        if (!string.IsNullOrWhiteSpace(newSpec))
            trainer.Specialization = newSpec;

        logger.Write($"ADMIN_UPDATE_TRAINER admin={currentAdmin!.Id} trainer={trainer.Id}");
        Console.WriteLine("ტრენერი წარმატებით განახლდა.");
        Input.Pause();
    }

    private void DeleteTrainer()
    {
        GetAllTrainers();
        var trainerId = Input.Read("ტრენერის ID რომელიც სურთ წაშლა: ");
        var trainer = trainers.Find(trainerId);

        if (trainer is null)
        {
            Console.WriteLine("ტრენერი ვერ მოიძებნა.");
            Input.Pause();
            return;
        }

        // Check if trainer has active sessions
        var activeSessions = sessions.Items.Where(s => s.TrainerId == trainerId && !s.IsCancelled).ToList();
        if (activeSessions.Count > 0)
        {
            Console.WriteLine($"ტრენერს აქვს {activeSessions.Count} აქტიური სესია. წაშლა შეუძლებელია.");
            Input.Pause();
            return;
        }

        trainers.Items.Remove(trainer);
        logger.Write($"ADMIN_DELETE_TRAINER admin={currentAdmin!.Id} trainer={trainerId}");
        Console.WriteLine("ტრენერი წარმატებით წაიშალა.");
        Input.Pause();
    }

    private void GetAllTrainers()
    {
        Console.WriteLine("=== ყველა ტრენერი ===");
        if (trainers.Items.Count == 0)
        {
            Console.WriteLine("ტრენერი ვერ მოიძებნა.");
            Input.Pause();
            return;
        }

        foreach (var trainer in trainers.Items)
        {
            var sessionCount = sessions.Items.Count(s => s.TrainerId == trainer.Id && !s.IsCancelled);
            Console.WriteLine($"{trainer.Id} | {trainer.FullName} | {trainer.Specialization} | {trainer.Email} | აქტიური სესიები: {sessionCount}");
        }
        Input.Pause();
    }

    public void Save()
    {
        files.Save("Members.txt", members.Items, member =>
            $"{member.Id}|{member.FullName}|{member.Email}");
        files.Save("Admins.txt", admins.Items, admin =>
            $"{admin.Id}|{admin.FullName}|{admin.Email}");
        files.Save("Trainers.txt", trainers.Items, trainer =>
            $"{trainer.Id}|{trainer.FullName}|{trainer.Specialization}|{trainer.Email}");
        files.Save("WorkoutSessions.txt", sessions.Items, session =>
            $"{session.Id}|{session.TrainerId}|{session.Type}|{session.Start:O}|" +
            $"{session.DurationMinutes}|{session.Capacity}|{session.IsCancelled}");
        files.Save("Bookings.txt", bookings.Items, booking =>
            $"{booking.Id}|{booking.MemberId}|{booking.SessionId}|{booking.CreatedAt:O}|{booking.IsCancelled}");
        files.Save("Visits.txt", visits.Items, visit =>
            $"{visit.Id}|{visit.MemberId}|{visit.Date:O}");
    }
}
