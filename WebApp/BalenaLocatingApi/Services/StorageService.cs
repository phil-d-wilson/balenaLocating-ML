using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BalenaLocatingApi.Data;
using BalenaLocatingApi.Models;
using Microsoft.Azure.Cosmos.Table;

namespace BalenaLocatingApi.Services
{
    public class StorageService
    {
        private readonly CloudTable _table;

        public StorageService()
        {
            _table = CreateTableAsync().Result;
        }

        public async Task InsertTrainingEntryAsync(TrainingEntry entity)
        {
            try
            {
                var insertOrMergeOperation = TableOperation.InsertOrMerge(entity);
                var result = await _table.ExecuteAsync(insertOrMergeOperation);
                var insertedCustomer = result.Result as TrainingEntry;

                if (result.RequestCharge.HasValue)
                {
                    Console.WriteLine("Request Charge of InsertOrMerge Operation: " + result.RequestCharge);
                }
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }

        public double[][] GetDataAsync()
        {
            var output = new List<double[]>();
            var entities = _table.ExecuteQuery(new TableQuery<TrainingEntry>());
            var index = 1;
            foreach (var entity in entities)
            {
                var newEntry = new double[]
                {
                    index++, entity.Device1, entity.Device2, entity.Device3,
                    (int) Enum.Parse(typeof(Locations), entity.Location)
                };

                output.Add(newEntry);
            }

            return output.ToArray();
        }

        private static async Task<CloudTable> CreateTableAsync()
        {
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse("TABLESTORAGECONNECTIONSTRING");
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the application.");
                throw;
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                Console.ReadLine();
                throw;
            }

            var tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            Console.WriteLine("Create a Table for the demo");

            // Create a table client for interacting with the table service 
            const string tableName = "TrainingData";
            var table = tableClient.GetTableReference(tableName);
            if (await table.CreateIfNotExistsAsync())
            {
                Console.WriteLine("Created Table named: {0}", tableName);
            }
            else
            {
                Console.WriteLine("Table {0} already exists", tableName);
            }

            Console.WriteLine();
            return table;
        }
    }
}
