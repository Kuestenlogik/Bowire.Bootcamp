using Grpc.Core;
using HelloGrpc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGrpc();
// Reflection lets Bowire's gRPC plugin walk the registered services
// at discovery time without needing the .proto file on the client.
builder.Services.AddGrpcReflection();

var app = builder.Build();
app.MapGrpcService<GreeterService>();
app.MapGrpcReflectionService();
app.Run("http://localhost:5002");

// Service implementation — generated base class comes from greeter.proto.
sealed class GreeterService : Greeter.GreeterBase
{
    public override Task<HelloReply> Hello(HelloRequest request, ServerCallContext context)
        => Task.FromResult(new HelloReply
        {
            Greeting = $"Hello, {request.Name}!",
            ReceivedAt = DateTimeOffset.UtcNow.ToString("O"),
        });

    public override async Task HelloStream(HelloRequest request,
        IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
    {
        var count = request.Count <= 0 ? 5 : Math.Min(request.Count, 20);
        for (var i = 1; i <= count; i++)
        {
            await responseStream.WriteAsync(new HelloReply
            {
                Greeting = $"Hello {i}/{count}, {request.Name}!",
                ReceivedAt = DateTimeOffset.UtcNow.ToString("O"),
            }, context.CancellationToken);
            // One frame per second so the workbench's streaming pane
            // shows them arriving — instant emission would look like
            // a single batch.
            await Task.Delay(1000, context.CancellationToken);
        }
    }
}
