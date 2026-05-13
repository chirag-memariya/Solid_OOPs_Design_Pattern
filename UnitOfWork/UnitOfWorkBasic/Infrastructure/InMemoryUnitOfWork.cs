using Microsoft.AspNetCore.Cors.Infrastructure;

public class InMemoryUnitOfWork : IUnitOfWork
{
    public Task SaveChangeAsync()
    {
        //in read : commit this to db
        Console.WriteLine("Changes saved!");
        return Task.CompletedTask;
    }
}