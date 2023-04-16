using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace RemoteHost;

[ApiController]
[Route("remote")]
public class RemoteController:ControllerBase
{
    [HttpGet("resend")]
    public async Task<string> Resend([FromQuery] long id)
    {
        HttpClient client = new ();
        client.BaseAddress = new Uri("https://localhost:44390/", UriKind.Absolute);
        var querybuilder = new QueryBuilder(
            new[]
            {
                new KeyValuePair<string, string>("id", id.ToString())
            });
        var name = await client.GetAsync("change-name"+querybuilder);
        return name.IsSuccessStatusCode
            ? name.Content.ToString()!
            : string.Empty;
    }
}