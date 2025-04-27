// Models/CollectionItem.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionManager.Models
{
    public class CollectionItem
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
        public int Satisfaction { get; set; }
        public string Comment { get; set; }
        public bool IsSold { get; set; } 

        public override string ToString()
        {
            return $"{Name} (Cena: {Price}, Status: {Status})";
        }
    }
}