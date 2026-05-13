namespace Delegates12;

// public class Program
// {
//     static void Print(string text)
//     {
//         Console.WriteLine(text);
//     }

//     public static void Main(string[] args)
//     {
//         Print("Hello world");
//     }

// }


// we can assing our fun to variable and can call it



public class Program
{
    public static void Print(string text)
    {
        Console.WriteLine(text);
    }

    public static void MainTest()
    {
        Program program = new Program();
        PrintDelegate printDelegate = Program.Print;
        printDelegate("Hello World! from Delegate");
    }

    delegate void PrintDelegate(string text);
}
