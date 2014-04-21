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
            Color color = Color.Black;
            String title = "";

            switch (Type)
            {
                case MenuType.MAIN:
                    title = "PATHOGENESIS";
                    color = new Color(90, 0, 0, 150);
                    break;
                case MenuType.PAUSE:
                    title = "Paused";
                    color = new Color(20, 0, 0, 150);
                    break;
                case MenuType.WIN:
                    title = "Victory!";
                    color = new Color(0, 0, 0, 100);
                    break;
                case MenuType.LOSE:
                    title = "You're dead.";
                    color = new Color(20, 0, 0, 150);
                    break;
            }

            // Background
            canvas.DrawSprite(Background, color,
                new Rectangle((int)center.X - canvas.Width / 2, (int)center.Y - canvas.Height / 2, canvas.Width, canvas.Height),
                new Rectangle(0, 0, Background.Width, Background.Height));

            // Title
            canvas.DrawText(title, new Color(220, 200, 80),
                 new Vector2((int)center.X, (int)center.Y - canvas.Height/2 + 100), "font3", true);

            // Options
            for (int i = 0; i < Options.Length; i++)
            {
                Color option_color = new Color(220, 200, 80);
                int x = (int) center.X;
                int y = (int) center.Y - (Options.Length * 60)/2 + i * 60;
                if (i == CurSelection)
                {
                    canvas.DrawSprite(Background, new Color(220, 200, 80),
                        new Rectangle(x + 150, y, 15, 15),
                        new Rectangle(0, 0, Background.Width, Background.Height));
                }
                canvas.DrawText(Options[i], option_color,
                    new Vector2(x, y), "font2", true);
            }
        }
    }
}
