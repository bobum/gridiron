using DataAccessLayer.SeedData;

namespace DataAccessLayer
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            await SeedDataRunner.RunAsync(args);
        }
    }
}
