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

        public bool Active { get; set; }

        public HUD(Texture2D infect, Texture2D health)
        {
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

        public void DrawLayerOne(GameCanvas canvas, List<GameUnit> units, Player player)
        {
            if (Active)
            {
                foreach (GameUnit unit in units)
                {
                    if (!unit.Exists) continue;

                    //Attack indicator
                    if (unit.AttackCoolDown != 0)
                    {
                        int range = unit.AttackRange + unit.Size / 2;
                        canvas.DrawSprite(InfectTexture,
                            new Color(70, 20, 20, (int)(unit.AttackCoolDown * 2.5)),
                            new Rectangle((int)unit.Position.X - range, (int)unit.Position.Y - range, range * 2, range * 2),
                            new Rectangle(0, 0, InfectTexture.Width, InfectTexture.Height));
                    }
                }
                if (player != null)
                {
                    //Infection range
                    canvas.DrawSprite(InfectTexture, new Color(30, 0, 0, 30), player.Position, new Vector2(1, 1), 0f);
                }
            }
        }

        public void DrawLayerTwo(GameCanvas canvas, List<GameUnit> units, Player player)
        {
            if (Active)
            {
                foreach (GameUnit unit in units)
                {
                    if (!unit.Exists) continue;

                    //Infected bar indicator
                    if (unit.InfectionVitality != unit.max_infection_vitality)
                    {
                        canvas.DrawSprite(HealthBarTexture, new Color(0, 50, 100, 200),
                                            new Rectangle((int)unit.Position.X - HealthBarTexture.Width / 2, (int)unit.Position.Y - 50, (int)(0.5*(unit.max_infection_vitality - unit.InfectionVitality)), 8),
                                            new Rectangle(0, 0, HealthBarTexture.Width, (int)(HealthBarTexture.Height * 0.8)));
                    }
                    if (unit.Type != UnitType.BOSS)
                    {
                        //Health bar
                        canvas.DrawSprite(HealthBarTexture, new Color(0, 50, 0, 100),
                            new Rectangle((int)unit.Position.X - HealthBarTexture.Width / 2, (int)unit.Position.Y - 30, (int)(0.5 * unit.Health), 8),
                            new Rectangle(0, 0, HealthBarTexture.Width, (int)(HealthBarTexture.Height * 0.8)));
                    }
                }
                if (player != null)
                {
                    //Player health
                    canvas.DrawSprite(HealthBarTexture, new Color(0, 50, 0, 200),
                                            new Rectangle((int)player.Position.X - HealthBarTexture.Width / 2, (int)player.Position.Y - 30, (int)(0.5 * player.Health), 8),
                                            new Rectangle(0, 0, HealthBarTexture.Width, (int)(HealthBarTexture.Height * 0.8)));

                    canvas.DrawSprite(HealthBarTexture, new Color(200, 50, 50, 250),
                        new Rectangle((int)(player.Position.X - canvas.Width/2 + 10),
                            (int)(player.Position.Y - canvas.Height / 2 + 10),
                            (int)(player.Health * 5), 30),
                        new Rectangle(0, 0, HealthBarTexture.Width, HealthBarTexture.Height));

                    canvas.DrawSprite(HealthBarTexture, new Color(50, 50, 200, 250),
                        new Rectangle((int)(player.Position.X + 10),
                            (int)(player.Position.Y - canvas.Height / 2 + 10),
                            (int)(player.InfectionPoints / 2), 30),
                        new Rectangle(0, 0, HealthBarTexture.Width, HealthBarTexture.Height));
                }
            }
        }
    }
}
