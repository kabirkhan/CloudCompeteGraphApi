using System;
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
    public static class GremlinQueryHttpTrigger
    {
        [FunctionName("GremlinQueryHttpTrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "gremlin")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            CosmosGraphClient cosmosGraphClient = new CosmosGraphClient();

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string query = data?.query;

            ActionResult res;
            if (query == null || query.Contains("drop") || query.Contains("add"))
            {
                res = new BadRequestObjectResult("Please pass a valid gremlin traversal query. Queries that add, update or delete vertices or edges are not allowed.");
            } 
            else 
            {
                var records = cosmosGraphClient.Query(query);
                res = new OkObjectResult(records);
            }
            return res;
        }
    }
}
