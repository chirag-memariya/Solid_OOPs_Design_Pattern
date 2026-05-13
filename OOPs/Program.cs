namespace oops;

public class Program
{
    public static void Main()
    {
        //#//Encapsulation//#//
        // BankAccount bankAccount = new BankAccount();
        // bankAccount.Deposit(100);
        // bankAccount.PrintBalance();
        // bankAccount.Withdraw(50);
        // bankAccount.PrintBalance();
        // Console.WriteLine(bankAccount.Balance);//controlled access
        // //bankAccount._balance = 100; // X ddnot allowed (private)


        // //#//Abstraction//#//
        // IEmailService email = new EmailService();

        // // Caller only sees Send and Schedule
        // email.Send("Hello World");
        // email.Schedule("Meeting Reminder", DateTime.Now.AddHours(1));

        // // X Not allowed: caller cannot access internal methods
        // // email.FormatMessage("Test");   // Compile error
        // // email.ConnectToSmtp();         // Compile error
        // // Abstraction: The caller only knows the contract (Send, Schedule), 
        // //  not the internal logic.

        // // Trick: You cannot call any extra methods in EmailService unless 
        // //  they’re part of IEmailService.




        // //#//Polymorphism//#//
        // // Overloaded methods
        // var email = new EmailService1();
        // email.Send("Hello");
        // email.Send("Hello","Greetings");
        // email.Send("Hello","Greetings","Alice");

        // // Overriding methods
        // BaseEmailService email1 = new GmailService();
        // BaseEmailService email2 = new OutlookService();
        // email1.Send("Hello via Gmail");    // Runtime decides Gmail’s override
        // email2.Send("Hello via Outlook");  // Runtime decides Outlook’s override
        // //#//Polymorphism//#//



    }
}