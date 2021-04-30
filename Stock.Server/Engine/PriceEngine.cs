using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace Stock.Server
{

    public class PriceEngine: IPriceEngine, IDisposable
    {
        private readonly ConcurrentDictionary<string, Stock> _lowestPriceHash = new ConcurrentDictionary<string, Stock>();
        private readonly BlockingCollection<Stock> _priceBlockingCollection = new BlockingCollection<Stock>();
        private readonly ILogger<IPriceEngine> _logger;
        private readonly Task Handler;
        
        public PriceEngine(ILogger<IPriceEngine> logger)
        {
            this._logger = logger;
            
            Handler = Task.Run(() => ProcessLowestPrice());
            
        }

        private void ProcessLowestPrice()
        {
            //start async listener on the blockingCollection forever 
            Parallel.ForEach(_priceBlockingCollection.GetConsumingEnumerable(), 
                /*
                new ParallelOptions() { 
                    MaxDegreeOfParallelism = someUpperBound               
                },
                */
            stock =>
            {                
                if(_lowestPriceHash.TryGetValue(stock.Name, out Stock existing))
                {
                    if(existing.Price > stock.Price)
                        _lowestPriceHash[stock.Name] = stock; //replace                    
                }
                else
                {
                    _lowestPriceHash.TryAdd(stock.Name, stock);
                }               
                
            });
 
        }

        /// <summary>
        /// must be thread safe and efficient to work in extreme load
        /// </summary>
        /// <param name="stock"></param>
        public async Task AddStock(Stock stock)
        {
            if (stock == null)
                throw new StockServerException("stock is missing");

            bool success = false;
            //********************************************
            // If too much pressure (too many attempts/failure in TryAdd),
            // we can assign dozen of blockingCollection and
            // switch between any of them arbitrary to reduce pressure
            //********************************************
            //attemt 3 times
            for (int attempt = 0; attempt < 3; attempt++)
            {
                success = _priceBlockingCollection.TryAdd(stock); 
                if (!success)                
                    await Task.Delay(50); //wait for releasing the blockingCollection, if any               
                else
                    break;
            }
            if (!success)
            {
                _logger.LogError("AddStock failed to add to blockingCollection");
            }

        }



        public decimal GetLowestPrice(Stock stock)
        {            
            if (_lowestPriceHash.TryGetValue(stock.Name, out Stock lowest))
                return lowest.Price;
           
            _logger.LogWarning($"Stock {stock.Name} was not found");
            return -1;

        }

        public IEnumerable<Stock> GetAllLowest() 
        {
            //option 1 - return direct list. Not sure             
            //return LowestPrice.Values;

            //option2 - should we need to returm immutable stocks of current state
            List<Stock> result = new List<Stock>();

            _lowestPriceHash.Values.Select(stock =>
            {
                result.Add(new Stock(stock));
                return true;
            });
            return result;

           
        }

        public void Dispose()
        {
            _priceBlockingCollection.CompleteAdding();
            Handler.Dispose();
        }
    }
}
