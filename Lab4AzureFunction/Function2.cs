using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Lab4AzureFunction
{
    public static class Function2
    {
        static string endpoint = @"https://lab2.documents.azure.com:443/";
        static string authKey = "u2z5JpiXmx7IPSIt0SswUPwkACpuw9ohl6aEgoVUlda5Z5P1z9dVQlWDa8sYU2VddBCBRr3CmxuI89Ym19gqdw==";


        [FunctionName("Function2")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                log.Info("C# HTTP trigger function processed a request.");
                // the person objects are free-form in structure
                List<dynamic> results = new List<dynamic>();
                // open the client's connection
                using (DocumentClient client = new DocumentClient(
                    new Uri(endpoint),
                    authKey,
                    new ConnectionPolicy
                    {
                        ConnectionMode = ConnectionMode.Direct,
                        ConnectionProtocol = Protocol.Tcp
                    }))
                {
                    // get a reference to the database the console app created
                    Database database = await client.CreateDatabaseIfNotExistsAsync(
                        new Database
                        {
                            Id = "UserDB"
                        });


                    
                    // parse query parameter
                    string mode = req.GetQueryNameValuePairs()
                    .FirstOrDefault(q => string.Compare(q.Key, "mode", true) == 0)
                    .Value;
                    string id = req.GetQueryNameValuePairs()
                        .FirstOrDefault(q => string.Compare(q.Key, "id", true) == 0)
                        .Value;


                    // Get request body
                    dynamic data = await req.Content.ReadAsAsync<object>();

                    // Set name to query string or body data
                    mode = mode ?? data?.mode;
                    id = id ?? data?.id;

                    if (mode == "viewReviewQueue")
                    {
                        string returnString = "";
                        //var SqlList = client.CreateDocumentQuery<VerifiedPicture>(UriFactory.CreateDocumentCollectionUri("UserDB", "PictureCollection"), "SELECT {'ID':q.id, 'Adress':q.adress} FROM PictureCollection q");

                        IEnumerable<VerifiedPicture> SqlList = client.CreateDocumentQuery<VerifiedPicture>(UriFactory.CreateDocumentCollectionUri("UserDB", "PictureCollection"));

                        foreach (var url in SqlList)
                        {
                            VerifiedPicture newUrl = url;
                            returnString += $"\n Id = {newUrl.Id} Adress = {newUrl.Adress}\n";
                        }

                        return mode == null
                        ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a function command on the query string or in the request body")
                        : req.CreateResponse(HttpStatusCode.OK, returnString);
                    }

                    /*else if (mode == "approve")
                    {
                        var movingItem = client.CreateDocumentQuery(UriFactory.CreateDocumentCollectionUri("UserDB", "PictureCollection"), $"SELECT q FROM PictureCollection q WHERE q.Id = {id}");
                        await client.CreateDocumentAsync(UriFactory.CreateDocumentUri("UserDB", "VerifiedPictureCollection", $"{id}"), movingItem.First());
                        await client.DeleteDocumentAsync(UriFactory.CreateDocumentUri("UserDB", "PictureCollection", $"{id}"));


                        return mode == null
                                        ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a function command on the query string or in the request body")
                                        : req.CreateResponse(HttpStatusCode.OK, "Approved object with id " + id);
                    }*/

                    /*else if (mode == "reject")
                    {
                        await client.DeleteDocumentAsync(UriFactory.CreateDocumentUri("UserDB", "PictureCollection", $"{id}"));
                        client.CreateDocumentQuery(UriFactory.CreateDocumentCollectionUri("UserDB", "PictureCollection"), $"SELECT q FROM PictureCollection q WHERE q.Id = {id}");

                        return mode == null
                                        ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a function command on the query string or in the request body")
                                        : req.CreateResponse(HttpStatusCode.OK, "Rejected object with id " + id);
                    }*/

                    else
                    {
                        return mode == null
                                        ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a function command on the query string or in the request body")
                                        : req.CreateResponse(HttpStatusCode.OK, "Not a valid command " + mode);
                    }
                }
            }
            catch(Exception e)
            {
                int x = 7;
                return req.CreateResponse(HttpStatusCode.OK);
            }
        }
    }

    public class VerifiedPicture
    {
        [JsonProperty(PropertyName = "Id")]
        public string Id { get; set; }
        public string Adress { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
