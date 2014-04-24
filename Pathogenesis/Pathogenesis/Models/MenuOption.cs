using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Pathogenesis.Models
{
    public class MenuOption
    {
        public String Text { get; set; }
        public List<MenuOption> Options;
        public int CurSelection { get; set; }

        public Vector2 Offset { get; set; }

        public MenuOption(String text, Vector2 offset, List<MenuOption> options)
        {
            Text = text;
            Options = options;
            Offset = offset;

            CurSelection = 0;
        }

        public void Draw(GameCanvas canvas, Vector2 center, Color color)
        {
            canvas.DrawText(Text, color,
                new Vector2(center.X + Offset.X, center.Y + Offset.Y), "font2", Offset.X == 0);

            for (int i = 0; i < Options.Count; i++)
            {
                if (i == CurSelection)
                {
                    Options[i].Draw(canvas, center, Menu.fontHighlightColor);
                }
                else
                {
                    Options[i].Draw(canvas, center, Menu.fontColor);
                }
            }
        }
    }
}
