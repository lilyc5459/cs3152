using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Pathogenesis.Models
{
    public class HUD
    {
        public Texture2D InfectTexture { get; set; }
        public Texture2D HealthBarTexture { get; set; }
        public Player Player { get; set; }

        public bool Active { get; set; }

        public HUD(Player player, Texture2D infect, Texture2D health)
        {
            Player = player;
            InfectTexture = infect;
            HealthBarTexture = health;
            Active = true;
        }

        public void Update(InputController input_controller)
        {
            if (input_controller.Toggle_HUD)
            {
                Active = !Active;
            }
        }

        public void Draw(GameCanvas canvas, List<GameUnit> units)
        {
            if (Active)
            {
                canvas.DrawSprite(InfectTexture, new Color(0, 0, 0, 100), Player.Position, new Vector2(1, 1), 0f);
                canvas.DrawSprite(HealthBarTexture, new Color(0, 50, 0, 100),
                                        new Rectangle((int)Player.Position.X - HealthBarTexture.Width / 2, (int)Player.Position.Y - 30, (int)(0.5 * Player.Health), 8),
                                        new Rectangle(0, 0, HealthBarTexture.Width, (int)(HealthBarTexture.Height * 0.8)));
                foreach (GameUnit unit in units)
                {
                    if (unit.Faction != UnitFaction.ALLY && unit.InfectionVitality != GameUnit.MAX_INFECTION_VITALITY)
                    {
                        canvas.DrawSprite(HealthBarTexture, new Color(0, 50, 100, 100),
                                            new Rectangle((int)unit.Position.X - HealthBarTexture.Width / 2, (int)unit.Position.Y - 50, (int)(0.5*(GameUnit.MAX_INFECTION_VITALITY - unit.InfectionVitality)), 8),
                                            new Rectangle(0, 0, HealthBarTexture.Width, (int)(HealthBarTexture.Height * 0.8)));
                    }
                    canvas.DrawSprite(HealthBarTexture, new Color(0, 50, 0, 100),
                        new Rectangle((int)unit.Position.X - HealthBarTexture.Width/2, (int)unit.Position.Y - 30, (int)(0.5 * unit.Health), 8),
                        new Rectangle(0, 0, HealthBarTexture.Width, (int)(HealthBarTexture.Height * 0.8))); 
                }
            }
        }
    }
}
