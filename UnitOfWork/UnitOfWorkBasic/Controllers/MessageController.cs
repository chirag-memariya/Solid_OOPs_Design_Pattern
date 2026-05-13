using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class MessageControleller: ControllerBase
{
    public readonly IMediator _mediator;
    public MessageControleller(IMediator mediator)
    {
        _mediator = mediator;
    }
    [HttpPost]
    public async Task<string> Create([FromBody] string text)
    {
        return await _mediator.Send(new CreateMessageCommand(text));
    }
}