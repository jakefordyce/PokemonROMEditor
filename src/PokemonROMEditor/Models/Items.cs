using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonROMEditor.Models
{
    public class TM
    {
        public string TMName { get; set; }
        public int TMNumber { get; set; }
        public int MoveID { get; set; }
        public int Price { get; set; }
    }

    public class TMCompatible
    {
        public int TMNumber { get; set; }
        public string TMMoveName { get; set; }
        public bool CanLearn { get; set; }
    }

    public class Shop
    {
        public string ShopName { get; set; }
        public ObservableCollection<Item> Items { get; set; }
    }

    public class Item
    {
        public Item(ItemType item)
        {
            ItemID = item;
            Price = 0;
        }
        public Item(ItemType item, int price)
        {
            ItemID = item;
            Price = price;
        }
        public ItemType ItemID { get; set; }
        public int Price { get; set; }
    }
}
