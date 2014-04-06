using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pathogenesis
{
    public class ItemController
    {
        public List<Item> Items { get; set; }

        public ItemController()
        {
            Items = new List<Item>();
        }

        public void Reset()
        {
            Items = new List<Item>();
        }

        public void AddItem(Item p)
        {
            Items.Add(p);
        }

        public void RemoveItem(Item p)
        {
            Items.Remove(p);
        }

        public void Draw(GameCanvas canvas)
        {
            foreach (Item it in Items)
            {
                it.Draw(canvas);
            }
        }
    }
}
