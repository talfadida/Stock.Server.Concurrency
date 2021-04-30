using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stock.Server;
using System;
using System.Collections.Generic;
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
                .ReadSource(@"stocks.json");

            _ = serviceProvider.
                             GetServices<IPriceSourceReader>()
                             .FirstOrDefault(s => s.SourceType == PriceSourceType.JsonWebSource)
                         .ReadSource(@"https://s3.amazonaws.com/test-data-samples/stocks.json");
            #region Test against dummy api 
            /*
            _ = Task.Run(async () =>
              {
                  while (true)
                  {
                      _ = serviceProvider.
                              GetServices<IPriceSourceReader>()
                              .FirstOrDefault(s => s.SourceType == PriceSourceType.JsonWebSource)
                          .ReadSource(@"http://localhost:14573/api/pricelist");
                      await Task.Delay(1000);
                  }
              });
            */
            #endregion
          
            System.Console.WriteLine("processing...");
           
            ////test
            for (int i = 0; i < 10; i++)
            {
                TestPrice("CLRO");
                await Task.Delay(200);
            }

            var priceMeter = serviceProvider.GetService<IPriceEngine>();
            var all = priceMeter.GetAllLowest();
            foreach (var p in all.Take(10))
            {
                System.Console.WriteLine($"{p.Name} = {p.Price}");
            }

            //System.Console.ReadKey();
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
