using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stock.Server;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Stock.Console
{
    class Program
    {
        static ServiceProvider serviceProvider;
        static async Task Main(string[] args)
        {
            serviceProvider = new ServiceCollection()
               .AddLogging()
               .AddSingleton<IPriceSourceReader, PriceJsonSourceReader>()
               .AddSingleton<IPriceSourceReader, PriceUrlSourceReader>()
               .AddSingleton<IPriceEngine, PriceEngine>()                
               .BuildServiceProvider();

            //init readers async once
            //in real usage - need to invoke via some timer based mehcnism

             
            System.Console.WriteLine("IPriceSourceReader initiating..");
            _ = serviceProvider.
                    GetServices<IPriceSourceReader>()
                    .FirstOrDefault(s => s.SourceType == PriceSourceType.JsonFile)
                .ReadSource(@"C:\Users\tfadida\Source\Repos\AccessFintech\Stock.Console\stocks.json");

            _ = serviceProvider.
                    GetServices<IPriceSourceReader>()
                    .FirstOrDefault(s => s.SourceType == PriceSourceType.JsonWebSource)
                .ReadSource(@"https://s3.amazonaws.com/test-data-samples/stocks.json");

                      
            System.Console.WriteLine("processing...");           

            //test
            for (int i = 0; i < 5; i++)
            {
                TestPrice("AAWW");
                await Task.Delay(2000);
            }
            
            System.Console.ReadKey();
        }

        private static void TestPrice(string name)
        {
            var priceMeter = serviceProvider.GetService<IPriceEngine>();
            Stock.Server.Stock testStock = new Server.Stock() { Name = name };
            var test = priceMeter.GetLowestPrice(testStock);
            System.Console.WriteLine(test);
           
        }
    }
}
