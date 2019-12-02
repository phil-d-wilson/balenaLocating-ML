using System.Threading.Tasks;
using BalenaLocatingApi.Models;
using BalenaLocatingApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BalenaLocatingApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TrainingController : ControllerBase
    {
        private readonly HubService _hubService;
        private readonly StorageService _storageService;

        private readonly ILogger<TrainingController> _logger;

        public TrainingController(ILogger<TrainingController> logger)
        {
            _logger = logger;
            _hubService = new HubService();
            _storageService = new StorageService();
        }

        [HttpGet]
        public async Task Get()
        {
            var result = await _hubService.SampleHubData("28851-396");
            var locationName = "Kitchen";


            var trainingEntity = new TrainingEntry
            {
                Device1 = result.DeviceValues.ContainsKey("56c9e0a87b4a795e09da5579420eed32") ? result.DeviceValues["56c9e0a87b4a795e09da5579420eed32"].Rssi : -1001,
                Device2 = result.DeviceValues.ContainsKey("4a3fde3f9fe79bb5ce47718413502b5f") ? result.DeviceValues["4a3fde3f9fe79bb5ce47718413502b5f"].Rssi : -1001,
                Device3 = result.DeviceValues.ContainsKey("abf31fdd6af3691cd640e84b68a12009") ? result.DeviceValues["abf31fdd6af3691cd640e84b68a12009"].Rssi : -1001,
                Location = locationName
            };

            await _storageService.InsertTrainingEntryAsync(trainingEntity);

        }
    }
}
