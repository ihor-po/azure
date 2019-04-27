using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Linq;
using System.Threading.Tasks;
using TableSearch.Services;

namespace TableSearch
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var table = new StorageService().Table1;

            var query = new TableQuery<BadMessage>()
            {
                TakeCount = 5
            }
            .Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "BadMessages"),
                    TableOperators.And,
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("Text", QueryComparisons.GreaterThanOrEqual, "A"),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("Text", QueryComparisons.LessThan, "C"))));

            TableContinuationToken token = null;

            TableQuerySegment<BadMessage> seg;
            do
            {
                seg = await table.ExecuteQuerySegmentedAsync(query, token);
                token = seg.ContinuationToken;
                foreach (var badMessage in seg.Results)
                {
                    Console.WriteLine(badMessage.Text);
                }

            } while (token != null);

            Console.ReadKey();
        }
    }
}
