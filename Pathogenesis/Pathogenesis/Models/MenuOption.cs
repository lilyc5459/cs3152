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
        private List<MenuOption> options;

        public Vector2 Offset { get; set; }

        public MenuOption(String text, Vector2 offset, List<MenuOption> options)
        {
            Text = text;
            this.options = options;
            Offset = offset;
        }

        public void Draw(GameCanvas canvas, Vector2 center, Color color)
        {
            bool centerText = false;
            if (Offset.X == 0)
            {
                centerText = true;
            }
            canvas.DrawText(Text, color,
                new Vector2(center.X + Offset.X, center.Y + Offset.Y), "font2", centerText);
        }
    }
}
