using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pathogenesis.Models;
using Microsoft.Xna.Framework;

namespace Pathogenesis.Controllers
{
    public class MenuController
    {
        private Dictionary<MenuType, Menu> menus;
        public Menu CurMenu;

        public MenuController(ContentFactory factory)
        {
            menus = factory.createMenus();
        }

        /*
         * Set the current menu to the specified type
         */
        public void LoadMenu(MenuType type)
        {
            CurMenu = menus[type];
        }

        /*
         * Update menu selection
         */
        public void Update(InputController input_controller)
        {
            if (input_controller.DownOnce)
            {
                CurMenu.CurSelection = (int)MathHelper.Clamp(CurMenu.CurSelection + 1, 0, CurMenu.Options.Length - 1);
            }
            if (input_controller.UpOnce)
            {
                CurMenu.CurSelection = (int)MathHelper.Clamp(CurMenu.CurSelection - 1, 0, CurMenu.Options.Length - 1);
            }
        }

        public void DrawMenu(GameCanvas canvas, Vector2 center)
        {
            CurMenu.Draw(canvas, center);
        }
    }
}
