using MediatR;

public class CreateMessageHandler : IRequestHandler<CreateMessageCommand, string>
{
    public readonly IMessageRepository _messageRepository;
    public readonly IUnitOfWork _unitOfWork;
    public CreateMessageHandler(IMessageRepository messageRepository,IUnitOfWork unitOfWork)
    {
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
    }
    public async Task<string> Handle(CreateMessageCommand request, CancellationToken cancellationToken)
    {
        var msg = new Message(request.Text);
        _messageRepository.Add(msg);
        
        await _unitOfWork.SaveChangeAsync();

        return $"Message '{msg.Text}' saved with Id {msg.Id}";
    }
}