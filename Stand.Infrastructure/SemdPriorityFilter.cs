using MassTransit;

namespace Stand.Infrastructure;

public class SemdValidatedPriorityFilter<T> :
    IFilter<PublishContext<T>>
    where T : class
{
    public async Task Send(
        PublishContext<T> context,
        IPipe<PublishContext<T>> next)
    {
        context.SetPriority((byte) 7);
    }
    public void Probe(ProbeContext context)
    {
        
    }
}