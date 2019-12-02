using System;
using System.Threading.Tasks;
using BalenaLocatingApi.Models;
using Microsoft.Azure.Cosmos.Table;

namespace BalenaLocatingApi.Services
{
    public class StorageService
    {
        private CloudTable _table;

        public StorageService()
        {
            _table = CreateTableAsync().Result;
        }

        public async Task<TrainingEntry> InsertTrainingEntryAsync(TrainingEntry entity)
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

                return insertedCustomer;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }

        private async Task<CloudTable> CreateTableAsync()
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
