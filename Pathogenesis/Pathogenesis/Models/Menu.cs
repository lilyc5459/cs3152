using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Pathogenesis.Models
{
    #region Enum
    public enum MenuType
    {
        MAIN,
        PAUSE,
        WIN,
        LOSE
    };
    #endregion

    public class Menu
    {
        public MenuType Type { get; set; }
        public Texture2D Background { get; set; }

        public String[] Options {get; set; }
        public int CurSelection { get; set; }

        public Menu(MenuType type, String[] options, Texture2D background)
        {
            Type = type;
            Options = options;
            CurSelection = 0;
            Background = background;
        }

        public void Draw(GameCanvas canvas, Vector2 center)
        {
            //TEMP
            Color color;
            String msg = "";
            if (Type == MenuType.WIN)
            {
                msg = "Victory! Press Enter to restart";
                color = new Color(0, 0, 0, 100);
            }
            else
            {
                msg = "You're dead. Press Enter to restart";
                color = new Color(20, 0, 0, 150);
            }

            canvas.DrawSprite(Background, color,
                new Rectangle((int)center.X - canvas.Width / 2, (int)center.Y - canvas.Height / 2, canvas.Width, canvas.Height),
                new Rectangle(0, 0, Background.Width, Background.Height));
            canvas.DrawText(msg, new Color(220, 200, 0),
                new Vector2((int)center.X - 170, (int)center.Y));
        }
    }
}
