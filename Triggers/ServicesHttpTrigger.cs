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
    public static class ServicesHttpTrigger
    {
        [FunctionName("ServicesHttpTrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "services/{service_id?}")] HttpRequest req,
            string service_id,
            ILogger log)
        {
            CosmosGraphClient cosmosGraphClient = new CosmosGraphClient();

            string cloud_id = req.Query["cloud_id"];
            string category_id = req.Query["category_id"];


            string q;
            string projectStr = cosmosGraphClient.BuildProjectString(new List<string>{
                "id", "label", "name", "short_description", "long_description", "uri", "icon_uri"
            });


            if (service_id != null)
            {
                q = String.Format(@"g.V('{0}')
                        .project(
                            'id', 'label', 'name', 'shortDescription',
                            'longDescription', 'uri', 'iconUri', 'relatedServices'
                        ).by('id').by('label').by('name').by('short_description')
                        .by('long_description').by('uri').by('icon_uri')
                        .by(coalesce(
                            out('related_service').project('id', 'name').by('id').by('name'),
                            constant([]))
                        )", service_id);
            }
            else if (category_id != null)
            {
                q = String.Format("g.V('{0}').in('belongs_to'){1}", category_id, projectStr);
            }
            else if (cloud_id != null)
            {
                var cloudIdRes = cosmosGraphClient.Query(String.Format("g.V('{0}').values('abbreviation')", cloud_id));
                if (cloudIdRes.Count == 1) 
                {
                    q = String.Format("g.V().hasLabel('{0}_category').in('belongs_to'){1}", cloudIdRes[0], projectStr);
                }
                else
                {
                    return new BadRequestObjectResult(String.Format("Invalid cloud_id parameter. {0} does not exist", cloud_id));
                }
            }
            else
            {
                return new BadRequestObjectResult("You must supply one of cloud_id, category_id or service_id");
            }

            var records = cosmosGraphClient.Query(q);
            return new OkObjectResult(records);
        }
    }
}
