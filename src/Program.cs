using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace DataGenerator
{
    class Program
    {
        static CosmosClient client;
        static Container container;

        //DeviceId (10 digit alphanumeric)
        //Building (A-D)
        //Floors (1-10)
        //Temp (22-30)
        static Bogus.Faker<TemperatureReading> tempReadings = new Bogus.Faker<TemperatureReading>().Rules((faker, temp) =>
        {
            temp.Id = Guid.NewGuid().ToString();
            temp.DeviceId = faker.Random.AlphaNumeric(10);
            temp.Building = faker.Random.String2(1, "ABCD");
            temp.Floor = faker.Random.Number(1, 10).ToString();
            temp.Temperature = faker.Random.Number(22, 30);
        });

        static TemperatureReading tempReading;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello Data Generator!");

            
            string connectionString = "AccountEndpoint=https://mjb-cosmos-monitor.documents.azure.com:443/;AccountKey=YOakxzPjlAoI6StJAH5MPNB9EaE3JB541DfFTppNOFcqbHQ1kedLf346NwTszjDalF1flljZpsukgjIxXDGzIA==;";
            string databaseName = "DeviceDB";
            string containerName = "DeviceData";
            string partitionKey = "/deviceId";

            client = new CosmosClient(connectionString);

            Database database = await client.CreateDatabaseIfNotExistsAsync(databaseName);

            ContainerProperties properties = new ContainerProperties
            {
                Id = containerName,
                PartitionKeyPath = partitionKey
            };

            container = await database.CreateContainerIfNotExistsAsync(properties, 400);
             
            bool loadData = true;


            while (loadData)
            {
                int i;
                //Generate 100 device readings.
                for(i=0;i<100;i++)
                {
                    tempReading = tempReadings.Generate();

                    await container.CreateItemAsync(tempReading, new PartitionKey(tempReading.DeviceId));
                    Console.WriteLine($"Device Reading #{i}, Building: {tempReading.Building}, Floor: {tempReading.Floor}, Temperature: {tempReading.Temperature}");
                }
 
                Console.WriteLine("Generate another 100 readings?\nPress [Enter] to continue, any other key to exit.");
                if (Console.ReadKey().Key != ConsoleKey.Enter)
                    loadData = false;
            }
        }
    }
    public class TemperatureReading
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "deviceId")]
        public string DeviceId { get; set; }

        [JsonProperty(PropertyName = "building")]
        public string Building { get; set; }

        [JsonProperty(PropertyName = "floor")]
        public string Floor { get; set; }

        [JsonProperty(PropertyName = "temperature")]
        public double Temperature { get; set; }
    }
}
