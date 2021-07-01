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
                MiddleEarthObject middleearthObject,
            [CosmosDB(
                    databaseName: "Middle-Earth",
                    collectionName: "Mordor",
                    CreateIfNotExists = true,
                    PartitionKey = "/_pk",
                    ConnectionStringSetting = "CosmoDBConnection")]
                IAsyncCollector<dynamic> cosmosDbDocuments,
                ILogger log)
        {
            if (middleearthObject.teamName.ToLower() != _teamName) return;

            if (middleearthObject.type.ToLower() != "ring") return;
            log.LogInformation($"We found a [Ring] with sub-type '{middleearthObject.subtype}', id: '{middleearthObject.id}'");

            if (middleearthObject.subtype.ToLower() != "uniquering") return;
            log.LogInformation($"We found a [Unique Ring]!!, id: '{middleearthObject.id}'");

            if (_listOfUniqueRings.Exists(id => id == middleearthObject.id))
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
                _listOfUniqueRings.Add(middleearthObject.id);
            }
        }

        private static string _teamName;
        public RingFinderFunction(IConfiguration config)
        {
            _teamName = config["TeamName"].ToLower();
        }
    }

}