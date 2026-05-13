using MediatR;

public record SayHelloCommand(string Name) : IRequest<string>;