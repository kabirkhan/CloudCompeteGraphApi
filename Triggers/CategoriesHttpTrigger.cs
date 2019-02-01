using System;
using System.Collections.Generic;
using System.IO;
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
    public static class CategoriesHttpTrigger
    {
        [FunctionName("CategoriesHttpTrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "categories")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            CosmosGraphClient cosmosGraphClient = new CosmosGraphClient();

            string cloud_id = req.Query["cloud_id"];

            
            string projectStr = cosmosGraphClient.BuildProjectString(new List<string>{
                "id", "label", "name"
            });
            string q;
            
            if (cloud_id != null) 
            {
                q = String.Format("g.V('{0}').in('source_cloud')", cloud_id);
            }
            else
            {
                q = "g.V().hasLabel('category')";
            }
            q += projectStr;

            var records = cosmosGraphClient.Query(q);
            return new OkObjectResult(records);
        }
    }
}
