// using Microsoft.AspNetCore.Mvc;

// [ApiController]
// [Route("[controller]")]
// public class TestController: ControllerBase
// {
//     private readonly IGuidService _s1;
//     private readonly IGuidService _s2;
//     public TestController(IGuidService s1,IGuidService s2)
//     {
//         _s1 = s1;
//         _s2 = s2;
//     }

//     [HttpGet("transient")]
//     public object Get()
//     {
//         return new
//         {
//             Service1 = _s1.GetGuid(),
//             Service2 = _s2.GetGuid(),
//         };
//     }
// }

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class TestController: ControllerBase
{
    private readonly IOperationTransient _operationTransient1;
    private readonly IOperationTransient _operationTransient2;
    private readonly IOperationScoped _operationScoped1;
    private readonly IOperationScoped _operationScoped2;
    private readonly IOperationSingleton _operationSingleton;
    public TestController(
        IOperationTransient operationTransient1,
        IOperationTransient operationTransient2,
        IOperationScoped operationScoped1,
        IOperationScoped operationScoped2,
        IOperationSingleton operationSingleton
    )
    {
        _operationTransient1 = operationTransient1;
        _operationTransient2 = operationTransient2;
        _operationScoped1 = operationScoped1;
        _operationScoped2 = operationScoped2;
        _operationSingleton = operationSingleton;
    }

    [HttpGet("Operation")]
    public Object Get()
    {
        return new
        {
          OperationTransient1 = "Transient1 :"+ _operationTransient1.OperationId,
          OperationTransient2 = "Transient2 :"+ _operationTransient2.OperationId,
          OperationScoped1 = "Scoped1 :"+_operationScoped1.OperationId,
          OperationScoped2 = "Scoped2 :"+_operationScoped2.OperationId,
          OperationSingleton = "Singleton :"+_operationSingleton.OperationId  
        };
    } 
}