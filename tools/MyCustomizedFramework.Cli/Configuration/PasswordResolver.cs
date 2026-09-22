using System.Text;

namespace MyCustomizedFramework.Cli.Configuration;

/// <summary>
/// Never accepts a password from the committed crudgen.config.json. Resolution order:
/// --password flag &gt; CRUDGEN_DB_PASSWORD environment variable &gt; masked interactive prompt.
/// </summary>
internal sealed class PasswordResolver(Func<string> promptForPassword)
{
    public const string EnvironmentVariableName = "CRUDGEN_DB_PASSWORD";

    public string Resolve(string? flagValue)
    {
        if (!string.IsNullOrWhiteSpace(flagValue))
        {
            return flagValue;
        }

        var fromEnvironment = Environment.GetEnvironmentVariable(EnvironmentVariableName);
        if (!string.IsNullOrWhiteSpace(fromEnvironment))
        {
            return fromEnvironment;
        }

        return promptForPassword();
    }

    public static string ReadMasked()
    {
        Console.Write("Password: ");
        var password = new StringBuilder();

        ConsoleKeyInfo key;
        while ((key = Console.ReadKey(intercept: true)).Key != ConsoleKey.Enter)
        {
            if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password.Remove(password.Length - 1, 1);
                    Console.Write("\b \b");
                }

                continue;
            }

            if (!char.IsControl(key.KeyChar))
            {
                password.Append(key.KeyChar);
                Console.Write('*');
            }
        }

        Console.WriteLine();
        return password.ToString();
    }
}
