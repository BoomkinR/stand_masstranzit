using Microsoft.AspNetCore.Mvc;
using Bogus;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Stand.Domain;
using Stand.Infrastructure;
using Stand.Infrastructure.Contracts;

namespace Stand.Api;

[ApiController]
[Route("[controller]")]
public class Controller: ControllerBase
{
    private readonly DocumentContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    public Controller(DocumentContext dbContext,
                      IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet("update")]
    public async Task Update()
    {

        var documents = new Faker<Document>().RuleFor(x => x.Name, f => f.Random.String2(10))
            .RuleFor(x => x.IsComplete, false)
            .Generate(10);

        await _dbContext.AddRangeAsync(documents);
        await _dbContext.SaveChangesAsync();
    }
    
    [HttpGet("start")]
    public async Task Start()
    {
        var documentsId = _dbContext.Documents.Select(x => x.Id).AsAsyncEnumerable();
        await foreach (var id in documentsId)
        {
            await _publishEndpoint.Publish(
                new DataAdded
                {
                    Id = id
                }, CancellationToken.None);
        }
        await _dbContext.SaveChangesAsync();
    }
    
    [HttpGet("change-name")]
    public async Task<string> ChangeName([FromQuery]long id)
    {
        var document = await _dbContext.Documents.FirstAsync(x => x.Id == id);

        var a = _dbContext.Database.CurrentTransaction;
        document.Name = "changed4"+DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return document.Name;
    }
}