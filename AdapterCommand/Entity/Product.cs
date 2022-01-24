using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdapterCommand.Entity
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal LastPrice { get; set; }
        public int CategoryId { get; set; }
        public string PathImage { get; set; }
        public Category Category { get; set; }
    }
}
