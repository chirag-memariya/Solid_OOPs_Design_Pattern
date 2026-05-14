using System;

// 1. Declare the delegate
delegate void Notifier(string message);

class Program
{
    // 2. Methods matching delegate signature
    static void Sms(string msg) => Console.WriteLine("SMS: " + msg);
    static void Email(string msg) => Console.WriteLine("Email: " + msg);

    static void Main(string[] args)
    {
        // 3. Assign method to delegate
        Notifier notifier = Sms;

        // 4. Add another method (multicast delegate)
        notifier += Email;

        // 5. Invoke delegate (calls Sms and then Email)
        notifier("Camera Event: Intrusion Detected");
        notifier -= Sms;
        notifier("removed sms");

    }
}
