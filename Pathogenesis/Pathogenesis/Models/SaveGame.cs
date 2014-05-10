using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pathogenesis.Models
{
    /*
     * Defines a save game object
     */
    public class SaveGame
    {
        public Player Player;
        public int Level;
        public DateTime Time;

        public SaveGame() { }

        public SaveGame(Player player, int level)
        {
            Player = player;
            Level = level;
            Time = DateTime.Today;
        }
    }
}
