using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Amazon.Batch;
using Amazon.Batch.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FormacionFargateService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        // GET: api/Api
        [HttpGet]
        public IEnumerable<string> Get()
        {
            List<string> all_documents = new List<string>();

            string clusterEndpoint = "docdb-2021-03-31-11-44-27.cluster-cihrsqslvehp.eu-west-1.docdb.amazonaws.com:27017";
            string template = "mongodb://{0}:{1}@{2}/sampledatabase?ssl=true&replicaSet=rs0&readpreference={3}";
            string username = "adminadmin";
            string password = "adminadmin";
            string readPreference = "secondaryPreferred";
            string connectionString = String.Format(template, username, password, clusterEndpoint, readPreference);

            string pathToCAFile = "rdscombinedcabundle_cert_out.p7b";

            // ADD CA certificate to local trust store
            // DO this once - Maybe when your service starts
            X509Store localTrustStore = new X509Store(StoreName.Root);
            X509Certificate2Collection certificateCollection = new X509Certificate2Collection();
            certificateCollection.Import(pathToCAFile);
            try
            {
                localTrustStore.Open(OpenFlags.ReadWrite);
                localTrustStore.AddRange(certificateCollection);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Root certificate import failed: " + ex.Message);
                throw;
            }
            finally
            {
                localTrustStore.Close();
            }

            var settings = MongoClientSettings.FromUrl(new MongoUrl(connectionString));
            var client = new MongoClient(settings);
            var database = client.GetDatabase("sample_database");
            var collection = database.GetCollection<BsonDocument>("tracking");
            var cursor = collection.Find(new BsonDocument()).ToCursor();

            while (cursor.MoveNext())
            {
                var documents = cursor.Current;
                foreach(BsonDocument document in documents)
                {
                    all_documents.Add(document.ToString());
                }
            }
            return all_documents;
        }

        // GET: api/Api/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Api
        [HttpPost]
        public string Post([FromBody] Models.ModeloS3 data)
        {
            AmazonBatchClient batchClient = new AmazonBatchClient();

            SubmitJobResponse submitJobResponse1 = batchClient.SubmitJobAsync(new SubmitJobRequest()
            {
                ContainerOverrides = new ContainerOverrides() //opciones de contenedor
                {
                    Command = new List<string>() { data.bucket, data.key } //input del contenedor
                },
                JobDefinition = "formacionBatch", //definicion de trabajo
                JobName = "trabajo-webservice", //nombre del trabajo
                JobQueue = "formacionBatch", //cola de trabajo
            }).Result;

            string clusterEndpoint = "docdb-2021-03-31-11-44-27.cluster-cihrsqslvehp.eu-west-1.docdb.amazonaws.com:27017";
            string template = "mongodb://{0}:{1}@{2}/sampledatabase?ssl=true&replicaSet=rs0&readpreference={3}";
            string username = "adminadmin";
            string password = "adminadmin";
            string readPreference = "secondaryPreferred";
            string connectionString = String.Format(template, username, password, clusterEndpoint, readPreference);

            string pathToCAFile = "rdscombinedcabundle_cert_out.p7b";

            // ADD CA certificate to local trust store
            // DO this once - Maybe when your service starts
            X509Store localTrustStore = new X509Store(StoreName.Root);
            X509Certificate2Collection certificateCollection = new X509Certificate2Collection();
            certificateCollection.Import(pathToCAFile);
            try
            {
                localTrustStore.Open(OpenFlags.ReadWrite);
                localTrustStore.AddRange(certificateCollection);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Root certificate import failed: " + ex.Message);
                throw;
            }
            finally
            {
                localTrustStore.Close();
            }

            var settings = MongoClientSettings.FromUrl(new MongoUrl(connectionString));
            var client = new MongoClient(settings);
            var database = client.GetDatabase("sample_database");
            var collection = database.GetCollection<BsonDocument>("tracking");
            collection.InsertOne(new BsonDocument
            {
                {"ip", HttpContext.Connection.RemoteIpAddress.ToString() },
                {"fecha", DateTime.Now },
                {"bucket", data.bucket },
                {"key", data.key },
            });

            return "Trabajo enviado";
        }

        // PUT: api/Api/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
