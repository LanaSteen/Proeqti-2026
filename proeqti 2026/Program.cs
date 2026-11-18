namespace proeqti_2026;

internal static class Program
{
    private static void Main()
    {

        Console.InputEncoding = System.Text.Encoding.UTF8;
        Console.OutputEncoding = System.Text.Encoding.UTF8;


		var system = new FitZoneSystem(
            new FileManager("Data"),
            new Logger("Data/log.txt"),
            new EmailService());

        system.Load();
        system.Seed();

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== FitZone - ფიტნეს კლუბის მართვის სისტემა ===");
            Console.WriteLine("1. რეგისტრაცია\n2. შესვლა\n0. გასვლა");

            switch (Input.Read("არჩევანი: "))
            {
                case "1": system.Register(); break;
                case "2": system.Login(); break;
                case "0": system.Save(); return;
                default:
                    Console.WriteLine("არასწორი არჩევანი.");
                    Input.Pause();
                    break;
            }
        }
    }
}