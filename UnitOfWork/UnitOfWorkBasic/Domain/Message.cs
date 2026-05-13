public class Message
{
    public Guid Id{get;} = Guid.NewGuid();
    public string Text{get;}
    public Message(string txt)
    {
        Text = txt;
    }
}