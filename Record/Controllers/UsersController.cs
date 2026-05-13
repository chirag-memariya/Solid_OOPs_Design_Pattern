using Microsoft.AspNetCore.Mvc;

namespace Record.Controller;

[ApiController]
[Route("api/[controller]")]
public class UserController: ControllerBase {

    [HttpPost("register")]
    public IActionResult Register(RegisterUserRequest request){
        var response = new RegisterUserResponse(
            Guid.NewGuid(),
            request.Name,
            request.Email,
            DateTime.UtcNow
        );
        return Ok(response);
    }

}

public record RegisterUserRequest(string Name,string Email);
public record RegisterUserResponse(Guid Id,string Name,string Email,DateTime CreatedAt);