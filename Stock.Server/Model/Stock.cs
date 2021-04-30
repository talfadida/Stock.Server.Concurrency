using System;

namespace Stock.Server
{
    public class Stock
    {
        public Stock()
        {

        }
        public Stock(Stock stock)
        {
            //clone
            this.Name = stock.Name;
            this.Price = stock.Price;
        }

        public string Name { get; set; }

        public Decimal Price { get; set; }
    }
}
