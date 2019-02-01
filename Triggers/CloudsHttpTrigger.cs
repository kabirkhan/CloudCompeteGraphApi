using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Cloud.Compete.Graph.Clients;

namespace Cloud.Compete.Graph
{
    public static class CloudsHttpTrigger
    {
        [FunctionName("CloudsHttpTrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "clouds")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string q = "g.V().hasLabel('cloud').project('id', 'name', 'abbreviation').by('id').by('name').by('abbreviation')";
            CosmosGraphClient cosmosGraphClient = new CosmosGraphClient();
            var records = cosmosGraphClient.Query(q);
            return new OkObjectResult(records);
        }
    }
}

