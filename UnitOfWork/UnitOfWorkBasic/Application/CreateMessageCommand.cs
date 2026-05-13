
using MediatR;

public record CreateMessageCommand(string Text) : IRequest<string>;