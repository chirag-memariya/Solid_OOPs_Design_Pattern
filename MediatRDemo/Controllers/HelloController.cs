using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class HelloController : ControllerBase
{
    private readonly IMediator _mediator;
    public HelloController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{name}")]
    public async Task<string> SayHello(string name)
    {
        return await _mediator.Send(new SayHelloCommand(name));
    }
    [HttpGet()]
    public async Task<string> SayHello1(string name)
    {
        return await _mediator.Send(new SayHelloCommand(name+"SayHello1"));
    }
    [HttpGet("hello2")]
    public async Task<string> SayHello2(string name)
    {
        return await _mediator.Send(new SayHelloCommand(name+"SayHello2"));
    }
}