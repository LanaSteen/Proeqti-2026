using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace proeqti_2026;

public sealed class Logger
{
    private readonly string path;

    public Logger(string path)
    {
        this.path = path;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    }

    public void Write(string message) => File.AppendAllText(
        path,
        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}{Environment.NewLine}");
}

public interface IEmailService
{
    void Send(string to, string subject, string body);
}

public sealed class EmailService : IEmailService
{
    public void Send(string to, string subject, string body)
    {
        var host = Environment.GetEnvironmentVariable("FITZONE_SMTP_HOST");
        if (string.IsNullOrWhiteSpace(host)) return;

        using var client = new SmtpClient(host, 587) { EnableSsl = true };
        client.Credentials = new NetworkCredential(
            Environment.GetEnvironmentVariable("FITZONE_SMTP_USER"),
            Environment.GetEnvironmentVariable("FITZONE_SMTP_PASSWORD"));
        client.Send(
            Environment.GetEnvironmentVariable("FITZONE_SMTP_FROM") ?? "fitzone@localhost",
            to,
            subject,
            body);
    }
}

public sealed class FileManager
{
    private readonly string folder;

    public FileManager(string folder)
    {
        this.folder = folder;
        Directory.CreateDirectory(folder);
    }

    public void Save<T>(string name, IEnumerable<T> items, Func<T, string> format) =>
        File.WriteAllLines(Path.Combine(folder, name), items.Select(format));
}

public static class Security
{
    public static string Hash(string value) => Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
