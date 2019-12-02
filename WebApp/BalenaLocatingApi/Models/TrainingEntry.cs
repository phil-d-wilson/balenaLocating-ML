using System;
using Microsoft.Azure.Cosmos.Table;

namespace BalenaLocatingApi.Models
{
    public class TrainingEntry : TableEntity
    {
        public TrainingEntry()
        {
            PartitionKey = "1";
            RowKey = Guid.NewGuid().ToString();
        }

        public string Location { get; set; }
        public int Device1 { get; set; }
        public int Device2 { get; set; }
        public int Device3 { get; set; }
    }
}
