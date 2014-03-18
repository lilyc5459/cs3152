using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pathogenesis
{
    public class ItemController
    {
        public List<Pickup> Items { get; set; }

        public ItemController()
        {
            Items = new List<Pickup>();
        }

        public void Reset()
        {
            Items = new List<Pickup>();
        }

        public void AddItem(Pickup p) {
            Items.Add(p);
        }

        public void RemoveItem(Pickup p)
        {
            Items.Remove(p);
        }

        public void Draw(GameCanvas canvas)
        {
            foreach (Pickup it in Items)
            {
                it.Draw(canvas);
            }
        }
    }
}
