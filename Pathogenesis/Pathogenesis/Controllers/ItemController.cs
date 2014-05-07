using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Pathogenesis
{
    public class ItemController
    {
        private Random rand;
        private ContentFactory factory;         // Instance of the content factory

        // Map of drop probabilities for each item
        public Dictionary<ItemType, float> DropProbabilities { get; set; }

        // List of all items in the game
        public List<Item> Items { get; set; }
        public List<Item> DestroyedItems { get; set; }

        // Previous positions of all items, used for collision
        public Dictionary<int, Vector2> PreviousPositions { get; set; }

        public ItemController(ContentFactory factory)
        {
            this.factory = factory;

            rand = new Random();
            DropProbabilities = new Dictionary<ItemType, float>();
            Items = new List<Item>();
            DestroyedItems = new List<Item>();
            PreviousPositions = new Dictionary<int, Vector2>();

            DropProbabilities.Add(ItemType.PLASMID, 0.5f);
            DropProbabilities.Add(ItemType.HEALTH, 0.3f);
            DropProbabilities.Add(ItemType.ALLIES, 0.1f);
            DropProbabilities.Add(ItemType.RANGE, 0.025f);
            DropProbabilities.Add(ItemType.INFECT_REGEN, 0.025f);
            DropProbabilities.Add(ItemType.MAX_HEALTH, 0.025f);
            DropProbabilities.Add(ItemType.MAX_INFECT, 0.025f);
            DropProbabilities.Add(ItemType.ATTACK, 0.0f);
            DropProbabilities.Add(ItemType.MYSTERY, 0.0f);
            DropProbabilities.Add(ItemType.SPEED, 0.0f);
        }

        /*
         * Update all items;
         */
        public void Update()
        {
            // Remove destroyed items
            foreach(Item item in DestroyedItems)
            {
                Items.Remove(item);
            }
            DestroyedItems.Clear();

            foreach (Item item in Items)
            {
                // Handle destroyed items
                if (item.Destroyed)
                {
                    DestroyedItems.Add(item);
                    continue;
                }

                // Update previous positions
                if (PreviousPositions.ContainsKey(item.ID))
                {
                    PreviousPositions[item.ID] = item.Position;
                }
                else
                {
                    PreviousPositions.Add(item.ID, item.Position);
                }

                // Update item position
                item.Position += item.Vel;
                Vector2 vel_mod = item.Vel;
                if (vel_mod.Length() > item.Decel / 2)
                {
                    vel_mod.Normalize();
                    vel_mod *= item.Decel;
                    item.Vel -= vel_mod;
                }
                else
                {
                    item.Vel = Vector2.Zero;
                }
            }
        }

        public void Reset()
        {
            Items = new List<Item>();
        }

        /*
         * Spawns an item at the specified location with probabilities according to the probability map
         */
        public void AddRandomItem(Vector2 position)
        {
            double target = rand.NextDouble();
            float index = 0;
            ItemType type = ItemType.PLASMID;
            foreach (ItemType t in Enum.GetValues(typeof(ItemType)))
            {
                index += DropProbabilities[t];
                if (index > target)
                {
                    type = t;
                    break;
                }
            }
            Item item = factory.createItem(position, type);

            // Randomize velocity
            Vector2 vel = new Vector2((float)rand.NextDouble() - 0.5f, (float)rand.NextDouble() - 0.5f);
            if (vel.Length() != 0)
            {
                vel.Normalize();
                vel *= (5 + (float)rand.NextDouble() * 5);
            }
            else
            {
                vel = new Vector2(0, 7);
            }
            item.Vel = vel;

            // Set position slightly outwards from source
            Vector2 normal = item.Vel;
            if(normal.Length() > 0) {
                normal.Normalize();
            }
            item.Position = position + normal * 30;

            Items.Add(item);
        }

        public void AddItem(Item p)
        {
            Items.Add(p);
        }

        public void RemoveItem(Item p)
        {
            Items.Remove(p);
        }

        public void Draw(GameCanvas canvas, bool top)
        {
            foreach (Item it in Items)
            {
                it.Draw(canvas, top);
            }
        }
    }
}
