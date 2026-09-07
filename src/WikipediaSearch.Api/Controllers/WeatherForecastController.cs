using Microsoft.AspNetCore.Mvc;

namespace WikipediaSearch.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WikipediaSearchController : ControllerBase
{
    [HttpGet(Name = "GetWikipediaSearch")]
    public string Get()
    {
        return "Hello World";
    }
}
