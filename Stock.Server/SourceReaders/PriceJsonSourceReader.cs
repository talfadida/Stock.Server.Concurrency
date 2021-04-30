using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace Stock.Server
{

    public class PriceJsonSourceReader : IPriceSourceReader
    {
        private readonly IPriceEngine _repo;
        private readonly ILogger<IPriceSourceReader> _logger;

        private readonly ConcurrentDictionary<string, bool> safeGuradOnFileSource =
            new ConcurrentDictionary<string, bool>();

        public PriceJsonSourceReader(IPriceEngine repo, ILogger<IPriceSourceReader> logger)
        {
            this._repo = repo;
            this._logger = logger;
        }

        public PriceSourceType SourceType => PriceSourceType.JsonFile;

        public async Task ReadSource(string source)
        {
            try
            {

                while (safeGuradOnFileSource.TryGetValue(source, out bool safe))
                {
                    _logger.LogWarning($"File {source} already under process"); //todo: need to write once. not every 100 ms
                    await Task.Delay(100);
                }

                if (safeGuradOnFileSource.TryAdd(source, true))
                {

                    await Task.Run(async () =>
                    {
                        Console.WriteLine("**** PriceJsonSourceReader scan started *** ");
                        using (StreamReader reader = File.OpenText(source))
                        {
                            JArray arrayOfStock = (JArray)JToken.ReadFrom(new JsonTextReader(reader));

                            foreach (var token in arrayOfStock.AsJEnumerable())
                            {
                                await _repo.AddStock(token.ToObject<Stock>());
                                //await Task.Delay(10); // for testing purpose
                            }

                        }
                        safeGuradOnFileSource.TryRemove(source, out bool _); //enable next scan on same file
                    });
                    Console.WriteLine("**** PriceJsonSourceReader scan done *** ");
                }
            }
            catch (Exception ex)
            {

                _logger.LogError($"PriceJsonSourceReader: Couldnt read from source {source} : {ex}");
                throw new StockServerException($"PriceJsonSourceReader: Couldnt read from source {source}");
            }
        }
    }
}
