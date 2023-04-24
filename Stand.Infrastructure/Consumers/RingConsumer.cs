using MassTransit;
using Microsoft.AspNetCore.Http.Extensions;
using Stand.Infrastructure.Contracts;

namespace Stand.Infrastructure.Consumers;

public class RingConsumerDefinition :
    ConsumerDefinition<RingConsumer>
{
    private readonly IServiceProvider _serviceProvider;

    public RingConsumerDefinition(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<RingConsumer> consumerConfigurator)
    {
        endpointConfigurator.UseEntityFrameworkOutbox<DocumentContext>(_serviceProvider);
    }
}

public class RingConsumer : IConsumer<Ringed>
{
    private readonly DocumentContext _dbContext;
    public RingConsumer(DocumentContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task Consume(ConsumeContext<Ringed> context)
    {
        var client = new HttpClient();
        client.BaseAddress = new Uri("https://localhost:44318/", UriKind.Absolute);
        var a = _dbContext.Database.CurrentTransaction;
        var querybuilder = new QueryBuilder(
            new[]
            {
                new KeyValuePair<string, string>("id", context.Message.Id.ToString())
            });
        
        await _dbContext.SaveChangesAsync();
        var name = await client.GetAsync("remote/resend" + querybuilder);
        Console.WriteLine("NAME Changed");
    }
}