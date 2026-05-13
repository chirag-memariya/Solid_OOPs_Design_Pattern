public class InMemoryMessageRepository : IMessageRepository
{
    public List<Message> Messages = new();
    public void Add(Message message)
    {
        Messages.Add(message);
    }
}