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
        LOSE,
        DIALOGUE,
    };
    #endregion

    public class Menu
    {
        public static Color fontColor = new Color(220, 200, 80);
        public static Color fontHighlightColor = new Color(250, 230, 200);
        public const int ANIMATION_TIME = 20;

        public MenuType Type { get; set; }
        public Texture2D Background { get; set; }

        public bool AnimatingIn { get; set; }
        public bool AnimatingOut { get; set; }
        public int Frame { get; set; }
        public int AnimationTime { get; set; }

        public List<MenuOption> Options { get; set; }
        public int CurSelection { get; set; }
        public String Text1 { get; set; }
        public String Text2 { get; set; }

        public List<MenuType> Children { get; set; }

        public Menu(MenuType type, List<MenuOption> options, List<MenuType> children, Texture2D background)
        {
            Type = type;
            Options = options;
            Children = children;
            Background = background;

            CurSelection = 0;

            if (type == MenuType.DIALOGUE) AnimationTime = ANIMATION_TIME;
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
                case MenuType.DIALOGUE:
                    color = new Color(50, 0, 0, 180);
                    break;
            }

            // Background
            if (Type == MenuType.DIALOGUE)
            {
                canvas.DrawSprite(Background, color * ((float)Frame/AnimationTime),
                    new Rectangle((int)center.X - canvas.Width / 2 + 20, (int)center.Y + canvas.Height / 7, canvas.Width - 40, 250),
                    new Rectangle(0, 0, Background.Width, Background.Height));
            }
            else
            {
                canvas.DrawSprite(Background, color,
                    new Rectangle((int)center.X - canvas.Width / 2, (int)center.Y - canvas.Height / 2, canvas.Width, canvas.Height),
                    new Rectangle(0, 0, Background.Width, Background.Height));
            }


            // Draw dialogue if applicable
            if (Type == MenuType.DIALOGUE && Text1 != null)
            {
                Color font_draw_color = fontColor * ((float)Frame / AnimationTime);
                canvas.DrawText(Text1, font_draw_color,
                    new Vector2((int)center.X, (int)center.Y + canvas.Height / 2 - 200), "font1", true);
                if (Text2 != null)
                {
                    canvas.DrawText(Text2, font_draw_color,
                        new Vector2((int)center.X, (int)center.Y + canvas.Height / 2 - 150), "font1", true);
                }
                canvas.DrawText("Enter >", font_draw_color,
                    new Vector2((int)center.X + 320, (int)center.Y + canvas.Height / 2 - 70), "font1", true);
                return;
            }

            // Title
            canvas.DrawText(title, fontColor,
                 new Vector2((int)center.X, (int)center.Y - canvas.Height / 2 + 100), "font3", true);

            // Options
            for (int i = 0; i < Options.Count; i++)
            {
                MenuOption option = Options[i];
                Color option_color = fontColor;
                if (i == CurSelection)
                {
                    option_color = fontHighlightColor;
                    canvas.DrawSprite(Background, option_color,
                        new Rectangle((int)(center.X + option.Offset.X - 150), (int)(center.Y + option.Offset.Y), 15, 15),
                        new Rectangle(0, 0, Background.Width, Background.Height));
                }
                option.Draw(canvas, center, option_color);
            }
        }
    }
}
