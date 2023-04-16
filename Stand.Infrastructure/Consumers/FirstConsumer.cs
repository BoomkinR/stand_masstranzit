using MassTransit;
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

        await context.Redeliver(
            TimeSpan.FromSeconds(5), (
                consumeContext,
                sendContext) =>
            {

            });
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
        // var client = new HttpClient();
        // client.BaseAddress = new Uri("https://localhost:44318/", UriKind.Absolute);
        // var a = _dbContext.Database.CurrentTransaction;
        // var querybuilder = new QueryBuilder(
        //     new[]
        //     {
        //         new KeyValuePair<string, string>("id", doc.Id.ToString())
        //     });
        //
        // await _dbContext.SaveChangesAsync();
        // var name = await client.GetAsync("remote/resend" + querybuilder);
        // Console.WriteLine(name.Content.ToString());
        //
        // // await Task.Delay(TimeSpan.FromSeconds(1));
        // var log = $"----------------------------------------------------\n" +
        //           $"MessageId: {context.MessageId}, IDS: {context.Message.Id}\n" +
        //           $"DbContext: {_dbContext.ContextId.InstanceId.ToString()}\n" +
        //           $"----------------------{DateTime.UtcNow}-----------------------------------";
        // Console.WriteLine(log);
        //
        // await context.Publish(
        //     new DataPerformed
        //     {
        //         Id = context.Message.Id
        //     });
        //
        // await _dbContext.SaveChangesAsync();
    }
}