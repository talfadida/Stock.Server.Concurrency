using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stock.Server
{
    public interface IPriceEngine
    {
        Task AddStock(Stock stock);

        decimal GetLowestPrice(Stock stock);

        IEnumerable<Stock> GetAllLowest();
    }
}