namespace Test;

internal class Program
{
    static void Main(string[] args)
    {
        string name = "chirag";
        string cap = name.Capitalize();
    }
}

public static class StringExtensions
{
    public static string Capitalize(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }
        return char.ToUpper(input[0]) + input.Substring(1);
    }
}