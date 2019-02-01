using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using Gremlin.Net.Driver;
using Gremlin.Net.Driver.Exceptions;
using Gremlin.Net.Structure.IO.GraphSON;


namespace Cloud.Compete.Graph.Clients
{
    public class CosmosGraphClient {
        private GremlinServer _gremlinServer;
        // public CosmosGraphClient(string hostname, string authKey, string database, string collection)
        public CosmosGraphClient()
        {
            string hostname = String.Format("{0}.gremlin.cosmosdb.azure.com", GetEnvironmentVariable("COSMOS_ACCOUNT_NAME"));
            int port = 443;
            string authKey = GetEnvironmentVariable("COSMOS_READ_KEY");
            string database = GetEnvironmentVariable("COSMOS_DB_NAME");
            string collection = GetEnvironmentVariable("COSMOS_GRAPH_NAME");

            this._gremlinServer = new GremlinServer(hostname, port, enableSsl: true, 
                                                    username: "/dbs/" + database + "/colls/" + collection, 
                                                    password: authKey);
        }

        public List<dynamic> Query(string query)
        {
            using (var gremlinClient = new GremlinClient(this._gremlinServer, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType))
            {
                var resultSet = gremlinClient.SubmitAsync<dynamic>(query).Result;

                var records = new List<dynamic>();
                if (resultSet.Count > 0)
                {
                    foreach (var result in resultSet)
                    {
                        records.Add(result);
                    }
                }
                return records;
            }
        }

        public string BuildProjectString(List<string> attributes)
        {
            string project = String.Format(".project('{0}'", camelCase(attributes[0]));
            string by = "";
            for (int i = 0; i < attributes.Count; i++)
            {
                
                if (i > 0)
                    project += String.Format(", '{0}'", camelCase(attributes[i]));
                by += String.Format(".by('{0}')", attributes[i]);
            }
            return project + ')' + by;
        }

        private string camelCase(string underScoreAttr)
        {
            string[] split = underScoreAttr.Split(new [] {"_"}, StringSplitOptions.RemoveEmptyEntries);
            string output = split[0];
            
            for (int i = 1; i < split.Length; i++)
            {
                string s = split[i];
                output += s.Substring(0, 1).ToUpper() + s.Substring(1);
            }
            return output;
        }

        private static string GetEnvironmentVariable(string name)
        {
            return name + ": " + 
                System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}