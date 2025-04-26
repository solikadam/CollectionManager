using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionManager.Models
{
    public class Collection
    {
        public string Name { get; set; }
        public List<CollectionItem> Items { get; set; } = new List<CollectionItem>();
    }
}
