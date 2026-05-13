//functional programing
namespace Delegates12;
public class Program12
{
    public static void MainTest()
    {
    ConnectToDatabase(PrintConsole);
    ConnectToDatabase(PrintToFile);
    }

    static void ConnectToDatabase(PrintDelegate log)
    {
        log("inserting a new record into the database");
        log("the record got inserted into the database");
    }

    static PrintDelegate PrintConsole = (string text) =>
    {
        Console.WriteLine(text);
    };

    static PrintDelegate PrintToFile = (string text) =>
    {
        File.AppendAllText("./logs.txt",text);
    };


    delegate void PrintDelegate(string text);
}
//basically just keep in mind you can functions which you can use and send in to other 
//functions as parameters just as you know in default var 