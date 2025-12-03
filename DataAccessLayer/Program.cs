using DataAccessLayer.SeedData;

namespace DataAccessLayer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await SeedDataRunner.RunAsync(args);
        }
    }
}
