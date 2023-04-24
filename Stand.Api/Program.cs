using System.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Stand.Infrastructure;
using Stand.Infrastructure.Consumers;
using Stand.Infrastructure.Contracts;

var builder = WebApplication.CreateBuilder(args);

var dbConnectionString = builder.Configuration.GetConnectionString("ServiceDataContext");

builder.Services.AddDbContext<DocumentContext>(
    options => options.UseNpgsql(
        dbConnectionString,
        x => x.MigrationsAssembly("Stand.Infrastructure")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(
    x =>
    {
        x.AddEntityFrameworkOutbox<DocumentContext>(
            o =>
            {
                o.QueryDelay = TimeSpan.FromSeconds(5);
                o.UsePostgres();
                o.UseBusOutbox(ot => ot.MessageDeliveryLimit = 500);
                o.DuplicateDetectionWindow = TimeSpan.Zero;
                // o.DuplicateDetectionWindow = TimeSpan.FromSeconds(9);
            });

        x.AddConsumer<FirstConsumer>(
            typeof(FirstConsumerDefinition), cfg =>
            {
                cfg.Options<BatchOptions>(
                    options =>
                        options.SetMessageLimit(2)
                            .SetTimeLimitStart(BatchTimeLimitStart.FromLast)
                            .SetConcurrencyLimit(10));
            });
        x.AddConsumer<SecondConsumer>(
            typeof(SecondConsumerDefinition));
        x.AddConsumer<RingConsumer>(
            typeof(RingConsumerDefinition));
        x.SetKebabCaseEndpointNameFormatter();
        x.AddDelayedMessageScheduler();
        x.UsingRabbitMq(
            (
                context,
                cfg) =>
            {
                using var serviceScope = context.CreateScope();
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<DocumentContext>();
                if (dbContext.Database.GetPendingMigrations().Any())
                    dbContext.Database.Migrate();
                cfg.UseDelayedMessageScheduler();
                cfg.ConfigureEndpoints(context);
                cfg.UsePublishFilter(typeof(SemdValidatedPriorityFilter<>), context);

                // cfg.UseDelayedRedelivery(r => r.Intervals(TimeSpan.FromSeconds(5)));

                cfg.Host(
                    builder.Configuration["Rabbit:Host"],
                    builder.Configuration["Rabbit:VHost"] ?? "/",
                    h =>
                    {
                        h.Username(builder.Configuration["Rabbit:Login"]);
                        h.Password(builder.Configuration["Rabbit:Password"]);
                    });
            });
    });


var app = builder.Build();

app.UsePathBase(builder.Configuration["PathBase"]);

app.UseRouting();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});
app.MapControllers();

await app.RunAsync();