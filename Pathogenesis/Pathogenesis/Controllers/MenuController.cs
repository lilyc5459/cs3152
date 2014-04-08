using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pathogenesis.Models;

namespace Pathogenesis.Controllers
{
    public class MenuController
    {
        private Dictionary<MenuType, Menu> menus; 

        #region Properties
        //public WinMenu 
        #endregion

        public MenuController(ContentFactory factory)
        {
            menus = new Dictionary<MenuType, Menu>();

            menus.Add(MenuType.WIN, factory.createMenu(MenuType.WIN));
        }
    }
}
