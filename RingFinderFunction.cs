using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace LordOfTheRing.Functions
{
    public class RingFinderFunction
    {
        public static List<string> _listOfUniqueRings = new List<string>();

        [FunctionName("RingFinder")]
        public async Task RunAsync(
            [ServiceBusTrigger("cracks-of-doom", "%TeamName%", Connection = "ServiceBusConnection")]
                MiddleEarthObject middleEarthObject,
            [CosmosDB(
                    databaseName: "Middle-Earth",
                    collectionName: "Mordor",
                    CreateIfNotExists = true,
                    PartitionKey = "/_pk",
                    ConnectionStringSetting = "CosmoDBConnection")]
                IAsyncCollector<dynamic> cosmosDbDocuments,
                ILogger log)
        {
            if (middleEarthObject.teamName.ToLower() != _teamName) return;

            if (middleEarthObject.type.ToLower() == "orcs")
            {
                log.LogError($"We are found orcs!! we are cancelling this journey :(.");
                _listOfUniqueRings = new List<string>();
                return;
            }

            if (middleEarthObject.type.ToLower() != "ring") return;

            if (middleEarthObject.subtype.ToLower() != "uniquering") return;
            log.LogInformation($"We found a [Unique Ring] 💍💍💍, id: '{middleEarthObject.id}'");

            if (_listOfUniqueRings.Exists(id => id == middleEarthObject.id))
            {
                log.LogWarning($"We found all the [Unique Rings]!! total unique rings: '{_listOfUniqueRings.Count}'.");
                var document = new
                {
                    Description = $"We found '{_listOfUniqueRings.Count}' unique Rings, "
                                + $"we are '{_teamName}' team [{DateTime.UtcNow.ToString("hh.mm.ss.ffffff")}]",
                    teamName = _teamName,
                    _pk = _teamName,
                    MachineName = Environment.MachineName,
                    id = Guid.NewGuid()
                };
                await cosmosDbDocuments.AddAsync(document);

                _listOfUniqueRings = new List<string>();
            }
            else
            {
                _listOfUniqueRings.Add(middleEarthObject.id);
            }
        }

        private static string _teamName;
        public RingFinderFunction(IConfiguration config)
        {
            if (string.IsNullOrEmpty(_teamName))
                _teamName = config["TeamName"].ToLower();
        }
    }

}