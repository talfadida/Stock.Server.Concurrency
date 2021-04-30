using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Stock.Server
{
    public class PriceUrlSourceReader : IPriceSourceReader
    {
        private readonly IPriceEngine _repo;
        private readonly ILogger<IPriceSourceReader> _logger;

        public PriceUrlSourceReader(IPriceEngine repo, ILogger<IPriceSourceReader> logger)
        {
            this._repo = repo;
            this._logger = logger;
        }

        public PriceSourceType SourceType => PriceSourceType.JsonWebSource;

        public async Task ReadSource(string source)
        {
            //this is not best practice! HttpClient instance should be created as singelton per source url 
            try
            {
                System.Console.WriteLine("**** PriceUrlSourceReader scan started *** ");

                using (var httpClient = new HttpClient())
                {
                    var urlSourceResp = await httpClient.GetAsync(source);
                    urlSourceResp.EnsureSuccessStatusCode();

                    var contentAsText = await urlSourceResp.Content.ReadAsStringAsync();
                    JArray arrayOfStock = (JArray)JToken.Parse(contentAsText);

                    foreach (var token in arrayOfStock.AsJEnumerable())
                    {
                        await _repo.AddStock(token.ToObject<Stock>());
                       // await Task.Delay(10); // for testing purpose
                    }
                    System.Console.WriteLine("**** PriceUrlSourceReader scan done *** ");

                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"PriceUrlSourceReader: Couldnt read from source {source} : {ex}");
                throw new StockServerException($"PriceUrlSourceReader: Couldnt read from source {source}");                
            }
        }
    }
}
