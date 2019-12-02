using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BalenaLocatingApi.Data;
using BalenaLocatingApi.Helpers;
using BalenaLocatingApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BalenaLocatingApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LocateController : ControllerBase
    {
        private readonly HubService _hubService;
        private readonly ClassificationService _classificationService;

        public LocateController()
        {
            _hubService = new HubService();
            _classificationService = new ClassificationService();
        }

        [HttpGet]
        public async Task<ContentResult> Get()
        {
            var result = await _hubService.SampleHubData("28851-396", 10);
            var formattedResult = DataConverter.Convert(result);
            var classification = _classificationService.Analyze(formattedResult);
            var output = (Locations) classification;

            return base.Content(output.ToString(), "text/html", Encoding.UTF8);
        }
    }
}