using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.ServiceBus;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace LordOfTheRing.Functions
{
    public class MiddlerEarthObjectsPopulatorFunction
    {
        public static List<string> _listOfUniqueRings = new List<string>();

        [FunctionName("MiddlerEarthObjectsPopulator")]
        public async Task<IActionResult> RunAsync(
                    [HttpTrigger(AuthorizationLevel.Anonymous, new[] { "post" })] HttpRequest req,
                    [ServiceBus("cracks-of-doom", Connection = "ServiceBusConnection", EntityType = EntityType.Topic)]
                    IAsyncCollector<MiddleEarthObject> middleEarthObjectCollector,
                    ILogger log)
        {
            if (req?.Body == null) return new BadRequestResult();

            try
            {
                using var stream = new StreamReader(req.Body);

                var requestBody = await stream.ReadToEndAsync();
                var middleEarthObject = JsonConvert.DeserializeObject<MiddleEarthObject>(requestBody);

                if (middleEarthObject == null) return new BadRequestResult();

                if (middleEarthObject.type.ToLower() != "orcs")
                {
                    await middleEarthObjectCollector.AddAsync(middleEarthObject);
                }
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

            return new OkResult();
        }


        private static string _teamName;
        public MiddlerEarthObjectsPopulatorFunction(IConfiguration config)
        {
            if (string.IsNullOrEmpty(_teamName))
                _teamName = config["TeamName"].ToLower();
        }
    }
}