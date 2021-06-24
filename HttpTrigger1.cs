using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
namespace Pipin.Function
{
    public static class PipinBuscador
    {
        public static List<string> _listOfUniqueRings = new List<string>();

        [FunctionName("PipinBuscador")]
        public  static async Task  RunPipinBuscador([ServiceBusTrigger("cracks-of-doom", "pipin", Connection = "katasbdevwe001_SERVICEBUS")] MiddleearthObject MiddleearthObject,
        [CosmosDB(
                databaseName: "Middle-Earth",
                collectionName: "Mordor",
                CreateIfNotExists = true, 
                PartitionKey = "/_pk",
                ConnectionStringSetting = "CosmoDBConnection")] 
               IAsyncCollector<dynamic> documents,
               ILogger log)
        {
            if (MiddleearthObject.type.ToLower() == "ring")
            {
                log.LogInformation($"Hemos encontrado {MiddleearthObject.id} anillo del subtipo {MiddleearthObject.subtype}"); 

                if (MiddleearthObject.subtype.ToLower() == "uniquering")
                {
                     log.LogInformation($"Y además hemos encontrado este unique Ring {MiddleearthObject.id}");

                     if(_listOfUniqueRings.Exists(id => id == MiddleearthObject.id))
                     {
                         var document = new { 
                             Description = $"Encontramos {_listOfUniqueRings.Count} unique Rings, somos el equipo pipin",
                             team = "Pipin",
                             _pk = "Pipin",
                             id = Guid.NewGuid() 
                        };
                        documents.AddAsync(document);

                        _listOfUniqueRings = new List<string>();
                     } 
                     else 
                     {
                        _listOfUniqueRings.Add(MiddleearthObject.id);
                     }
                }
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