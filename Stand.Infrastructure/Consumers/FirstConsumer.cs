using MassTransit;
using MassTransit.Introspection;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.EntityFrameworkCore;
using Stand.Infrastructure.Contracts;

namespace Stand.Infrastructure.Consumers;

public class FirstConsumerDefinition :
    ConsumerDefinition<FirstConsumer>
{
    private readonly IServiceProvider _serviceProvider;

    public FirstConsumerDefinition(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<FirstConsumer> consumerConfigurator)
    {
        endpointConfigurator.UseEntityFrameworkOutbox<DocumentContext>(_serviceProvider);
        // endpointConfigurator.UseMessageRetry(r =>
        // {
        //     r.Immediate(2);
        //     r.Ignore(typeof(EndpointException));
        // });
        endpointConfigurator.UseDelayedRedelivery(
            opt =>
            {
                opt.Interval(100,TimeSpan.FromSeconds(2));
                opt.Handle(typeof(EndpointException));
            });
    }
}

public class FirstConsumer : IConsumer<DataAdded>
{
    private readonly DocumentContext _dbContext;

    public FirstConsumer(DocumentContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task Consume(ConsumeContext<DataAdded> context)
    {
        // var msg = context;
        // var doc = await _dbContext.Documents.FirstAsync(x => x.Id == msg.Message.Id-100);
        await Task.Delay(TimeSpan.FromMilliseconds(200));
        var a = context.Headers.TryGetHeader("MT-Redelivery-Count", out var hz);
        if (a)
        {
            Console.WriteLine("ПРИХОД ПРИШЕЛ - ЕПТА  + "+DateTime.Now);
            return;
        }
        if (context.Message.Id == 1)
        {
            Console.WriteLine(context.MessageId + "  -  "+ DateTime.Now);
            throw new EndpointException();
        }

        await Task.Delay(TimeSpan.FromSeconds(1));
        await _dbContext.SaveChangesAsync();

        // var msg = context;
        //     var doc = await _dbContext.Documents.FirstAsync(x => x.Id == msg.Message.Id);
        //
        //     if (msg.Message.Id % 10 == 0)
        //     {
        //         await Task.Delay(TimeSpan.FromSeconds(20));
        //     }
        //     if (msg.Message.Id % 2 != 0)
        //     {
        //         await Task.Delay(100);
        //     }
        //
        //     doc.IsComplete = !doc.IsComplete;
        //
        //
        // // await Task.Delay(TimeSpan.FromSeconds(1));
        // var log = $"----------------------------------------------------\n" +
        //           $"MessageId: {context.MessageId}, IDS: {context.Message.Id}\n" +
        //           $"DbContext: {_dbContext.ContextId.InstanceId.ToString()}\n" +
        //           $"----------------------{DateTime.UtcNow}-----------------------------------";
        // Console.WriteLine(log);
        //
        //
        // // await Ring(context);
        //
        // await context.Publish(
        //     new DataPerformed
        //     {
        //         Id = context.Message.Id
        //     });
        //
        // await _dbContext.SaveChangesAsync();
    }
    public async Task Ring(ConsumeContext<DataAdded> context)
    {
        // var client = new HttpClient();
        // client.BaseAddress = new Uri("https://localhost:44318/", UriKind.Absolute);
        // var a = _dbContext.Database.CurrentTransaction;
        // var querybuilder = new QueryBuilder(
        //     new[]
        //     {
        //         new KeyValuePair<string, string>("id", context.Message.Id.ToString())
        //     });
        //
        // await _dbContext.SaveChangesAsync();
        // var name = await client.GetAsync("remote/resend" + querybuilder);
        // Console.WriteLine("NAME Changed");
    }
}