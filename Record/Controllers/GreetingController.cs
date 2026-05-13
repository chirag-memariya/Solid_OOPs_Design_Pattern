using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class GreetingController : ControllerBase{
    private readonly IGreetingService _greetingService;
    public GreetingController(IGreetingService greetingService) => _greetingService = greetingService;

    //Attribute routed action
    [HttpGet("{name}")]
    public ActionResult<GreetingResponse> GetGreeting([FromRoute] string name, [FromQuery] int times = 1)
    {
        var model = new GreetingRequest { Name = name, Times = times };
        //model validation happend automatically because of [Apicontroller]
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var text = _greetingService.CreateGreeting(model);
        return Ok(new GreetingResponse { Message = text });
    }
    
    
    [HttpPost]
    public ActionResult<GreetingResponse> PostGreeting([FromBody] GreetingRequest request){
        if(!ModelState.IsValid) return ValidationProblem(ModelState);
        return Ok(new GreetingResponse {Message = _greetingService.CreateGreeting(request)});
    }

}

public record GreetingRequest{
    public string Name {get;init;} = "";
    public int Times{get;init;} = 1;
}

public record GreetingResponse{
    public string Message {get;init;} = "";
}

public interface IGreetingService{string CreateGreeting(GreetingRequest request);}

public class GreetingService : IGreetingService{
    public string CreateGreeting(GreetingRequest request){
        return string.Join(" ",Enumerable.Range(0,Math.Max(1,request.Times))
            .Select( _ => $"Hello, {request.Name}!")
            );
    }
}