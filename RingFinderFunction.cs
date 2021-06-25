using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
namespace Pipin.Function
{
    public static class RingFinderFunction
    {
        private const string TeamName = "XXXXTeamNameXXXX";
        private const string TeamNameFunctionName = "XXXXTeamNameXXXX" + "RingFinder";
        public static List<string> _listOfUniqueRings = new List<string>();

        [FunctionName(TeamNameFunctionName)]
        public static async Task RunFinderFunction(
            [ServiceBusTrigger("cracks-of-doom", TeamName, Connection = "katasbdevwe001_serviceBus")] 
                MiddleearthObject middleearthObject,
            [CosmosDB(
                    databaseName: "Middle-Earth",
                    collectionName: "Mordor",
                    CreateIfNotExists = true, 
                    PartitionKey = "/_pk",
                    ConnectionStringSetting = "CosmoDBConnection")] 
                IAsyncCollector<dynamic> cosmosDbDocuments,
                ILogger log)
        {
            if (middleearthObject.type.ToLower() != "ring") return;

            log.LogInformation($"We found a [Ring] with sub-type '{middleearthObject.subtype}', id: '{middleearthObject.id}'"); 

            if (middleearthObject.subtype.ToLower() != "uniquering") return;
            
            log.LogInformation($"We found a [Unique Ring]!!, id: '{middleearthObject.id}'");

            if(_listOfUniqueRings.Exists(id => id == middleearthObject.id))
            {
                log.LogWarning($"We found all the [Unique Rings]!! total unique rings: '{_listOfUniqueRings.Count}'.");
                var document = new 
                { 
                    Description = $"We found '{_listOfUniqueRings.Count}' a unique Rings, we are '{TeamName}' team",
                    team = TeamName,
                    _pk = TeamName,
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
    }

    public class MiddleearthObject 
    {
        public string id { get; set; }
        public string type { get; set; }
        public string subtype { get; set; }
    }
}