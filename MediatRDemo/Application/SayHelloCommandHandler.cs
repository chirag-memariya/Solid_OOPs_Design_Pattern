using MediatR;

public class SayHelloCommandHandler : IRequestHandler<SayHelloCommand, string>
{
    public Task<string> Handle(SayHelloCommand request,CancellationToken cancellationToken)
    {
        return Task.FromResult($"Hello, {request.Name}"!);
    }
}