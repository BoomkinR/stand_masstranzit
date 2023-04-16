using MassTransit;
using Stand.Infrastructure.Contracts;

namespace Stand.Infrastructure.Consumers;



public class SecondConsumerDefinition :
    ConsumerDefinition<SecondConsumer>
{
    private readonly IServiceProvider _serviceProvider;

    public SecondConsumerDefinition(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<SecondConsumer> consumerConfigurator)
    {
        endpointConfigurator.UseEntityFrameworkOutbox<DocumentContext>(_serviceProvider);
    }
}

public class SecondConsumer:IConsumer<DataPerformed>
{

    public async Task Consume(ConsumeContext<DataPerformed> context)
    {
        await Task.Delay(TimeSpan.FromSeconds(1));
        var log = $"-------------------------SECOND-CONSUMER-------------------------------\n" +
                  $"ID: {context.Message.Id}\n" +
                  $"-----------------------------------------------------------------------------\n";
        Console.WriteLine(log);
    }
}