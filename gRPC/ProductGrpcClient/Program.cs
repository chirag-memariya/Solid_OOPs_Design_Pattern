using Grpc.Net.Client;
using ProductGrpcServer.Protos;

// Use HTTP for development (not HTTPS)
using var channel = GrpcChannel.ForAddress("http://localhost:5092", new GrpcChannelOptions
{
    HttpHandler = new System.Net.Http.HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = 
            System.Net.Http.HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    }
});
var client = new ProductService.ProductServiceClient(channel);

// CREATE PRODUCT
var created = await client.CreateProductAsync(
    new CreateProductRequest
    {
        Name = "Mechanical Keyboard",
        Price = 4500
    });

Console.WriteLine($"Created: {created.Id}");

// GET PRODUCT (DB)
var firstRead = await client.GetProductAsync(
    new GetProductRequest { Id = created.Id });

Console.WriteLine($"Source: {firstRead.Source}");

// GET PRODUCT (CACHE)
var secondRead = await client.GetProductAsync(
    new GetProductRequest { Id = created.Id });

Console.WriteLine($"Source: {secondRead.Source}");
