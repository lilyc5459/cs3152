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
        OPTIONS,
        WIN,
        LOSE
    };
    #endregion

    public class Menu
    {
        public MenuType Type { get; set; }
        public Texture2D Background { get; set; }

        public List<MenuOption> Options { get; set; }
        public int CurSelection { get; set; }

        public List<MenuType> Children { get; set; }

        public Menu(MenuType type, List<MenuOption> options, List<MenuType> children, Texture2D background)
        {
            Type = type;
            Options = options;
            Children = children;
            Background = background;

            CurSelection = 0;
        }

        public void Draw(GameCanvas canvas, Vector2 center)
        {
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
                case MenuType.OPTIONS:
                    title = "Options";
                    color = new Color(90, 0, 0, 250);
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
            for (int i = 0; i < Options.Count; i++)
            {
                MenuOption option = Options[i];
                Color option_color = new Color(220, 200, 80);
                if (i == CurSelection)
                {
                    canvas.DrawSprite(Background, new Color(220, 200, 80),
                        new Rectangle((int)(center.X + option.Offset.X - 150), (int)(center.Y + option.Offset.Y), 15, 15),
                        new Rectangle(0, 0, Background.Width, Background.Height));
                }
                option.Draw(canvas, center, option_color);
            }
        }
    }
}
