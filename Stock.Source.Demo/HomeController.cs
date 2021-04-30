using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stock.Source.Demo
{
    [ApiController]
    public class HomeController : ControllerBase
    {
        public HomeController()
        {

        }

        [Route("api/pricelist")]
        [HttpGet]
        public async Task<IEnumerable<Stock>> GetPrices()
        {
            return await Task.Run(() =>
            {
                List<Stock> result = new List<Stock>();
                Random rnd = new Random();

                for (int i = 0; i < 100; i++)
                {
                    var price = Decimal.Parse($"{rnd.Next(20, 80)}.{rnd.Next(20, 80)}");
                    result.Add(new Stock() { Name = $"A{i}", Price = price });
                }

                return result;
            });
        }

         

    }
}
