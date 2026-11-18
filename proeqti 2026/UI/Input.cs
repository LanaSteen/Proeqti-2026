namespace proeqti_2026;

public static class Input
{
    public static string Read(string label)
    {
        Console.Write(label);
        return Console.ReadLine()?.Trim() ?? "";
    }

    public static int Int(string label, int min, int max)
    {
        while (true)
        {
            if (int.TryParse(Read(label), out var number) && number >= min && number <= max)
                return number;

            Console.WriteLine("მიუთითეთ სწორი რიცხვი.");
        }
    }

    public static void Pause()
    {
        Console.WriteLine("\nგასაგრძელებლად დააჭირეთ Enter-ს...");
        Console.ReadLine();
    }
}
