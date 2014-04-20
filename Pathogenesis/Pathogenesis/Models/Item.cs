using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Pathogenesis
{
    #region Enums
    public enum ItemType
    {
        PLASMID,    // Gives conversion juice
        HEALTH,     // Gives health 
        ATTACK,     // Increases ally attack for a short period
        ALLIES,     // Gives 3 free allies
        RANGE,      // Increases infection range
        SPEED,      // Increases speed
        MAX_HEALTH, // Increases max health
        MAX_INFECT, // Increases max infect points
        INFECT_REGEN,   // Increases infection points regeneration speed
        MYSTERY,    // Mystery effect, can be positive or negative
    };
    #endregion

    public class Item : GameEntity
    {
        public const int ITEM_SIZE = 30;

        public Texture2D Texture { get; set; }

        public ItemType Type { get; set; }

        public Item(Texture2D texture, ItemType type)
        {
            Texture = texture;
            Type = type;
        }

        public void Draw(GameCanvas canvas)
        {
            Color color = Color.White;
            switch (Type)
            {
                case ItemType.PLASMID:
                    color = Color.Blue;
                    break;
                case ItemType.HEALTH:
                    color = Color.White;
                    break;
                case ItemType.ATTACK:
                    color = Color.Yellow;
                    break;
                case ItemType.ALLIES:
                    color = Color.Green;
                    break;
                case ItemType.RANGE:
                    color = Color.Orange;
                    break;
                case ItemType.SPEED:
                    color = Color.Yellow;
                    break;
            }
            canvas.DrawSprite(Texture, color,
                new Rectangle((int)Position.X - ITEM_SIZE/2, (int)Position.Y - ITEM_SIZE/2, ITEM_SIZE, ITEM_SIZE),
                new Rectangle(0, 0, Texture.Width, Texture.Height));
        }
    }
}
