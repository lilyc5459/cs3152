using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Timers;
using System.Diagnostics;

namespace Pathogenesis.Models
{
    public class HUD
    {
        public const int MINIMAP_TILE_SIZE = 4;
        public const int MINIMAP_WIDTH = 300;
        public const int MINIMAP_HEIGHT = 150;

        public const int PLAYER_MSG_TIME = 700;
 
        public Texture2D InfectTexture { get; set; }
        public Texture2D HealthBarTexture { get; set; }

        public bool Active { get; set; }

        public String PlayerMsg { get; set; }         // Floating msg indicator above player

        private Stopwatch stopwatch;

        public HUD(Texture2D infect, Texture2D health)
        {
            InfectTexture = infect;
            HealthBarTexture = health;
            Active = true;

            stopwatch = new Stopwatch();
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
                    if (unit.AttackCoolDown != 0 && unit.Type != UnitType.FLYING)
                    {
                        int range = unit.AttackRange + unit.Size / 2;
                        canvas.DrawSprite(InfectTexture,
                            new Color(70, 20, 20, (int)(unit.AttackCoolDown * 2.5)),
                            new Rectangle((int)unit.Position.X - range, (int)unit.Position.Y - range, range * 2, range * 2),
                            new Rectangle(0, 0, InfectTexture.Width, InfectTexture.Height));
                    }
                    canvas.DrawSprite(InfectTexture,
                            new Color(0, 0, 0, 100),
                            new Rectangle((int)unit.Position.X - (unit.Size+10)/2 + 10, (int)unit.Position.Y + 10, unit.Size + 10, unit.Size/2),
                            new Rectangle(0, 0, InfectTexture.Width, InfectTexture.Height));
                }
                if (player != null)
                {
                    //Infection range
                    int range = player.InfectionRange;
                    canvas.DrawSprite(InfectTexture, new Color(30, 0, 0, 30),
                        new Rectangle((int)player.Position.X - range, (int)player.Position.Y - range, range * 2, range * 2),
                        new Rectangle(0, 0, InfectTexture.Width, InfectTexture.Height));
                }
            }
        }

        public void DrawLayerTwo(GameCanvas canvas, List<GameUnit> units, Player player, Vector2 center, Level level)
        {
            if (!Active) return;
            foreach (GameUnit unit in units)
            {
                if (!unit.Exists) continue;

                //Infected bar indicator
                if (unit.InfectionVitality != unit.max_infection_vitality)
                {
                    canvas.DrawSprite(HealthBarTexture, new Color(0, 50, 100, 200),
                                        new Rectangle((int)unit.Position.X - HealthBarTexture.Width / 2, (int)unit.Position.Y - 50, (int)MathHelper.Lerp(50, 0, unit.InfectionVitality/unit.max_infection_vitality), 8),
                                        new Rectangle(0, 0, HealthBarTexture.Width, (int)(HealthBarTexture.Height * 0.8)));
                }
            }
            if (player == null) return;
            // Health
            canvas.DrawText("Health", new Color(220, 200, 80),
                new Vector2(center.X - canvas.Width / 2 + 10, center.Y - canvas.Height / 2 + 15),
                "font2", false);
            canvas.DrawSprite(HealthBarTexture, new Color(200, 50, 50, 250),
                new Rectangle((int)(center.X - canvas.Width/2 + 150),
                    (int)(center.Y - canvas.Height / 2 + 10),
                    (int)(MathHelper.Lerp(0, 500, player.Health/player.max_health)), 30),
                new Rectangle(0, 0, HealthBarTexture.Width, HealthBarTexture.Height));
            /*
            canvas.DrawText(player.Health + "/" + player.max_health, Color.White,
                new Vector2(player.Position.X - canvas.Width / 2 + 10, player.Position.Y - canvas.Height / 2 + 10),
                "font1", false);
                * */
                    
            // Infection points
            canvas.DrawText("Infect", new Color(220, 200, 80),
                new Vector2(center.X - canvas.Width / 2 + 10, center.Y - canvas.Height / 2 + 55),
                "font2", false);
            canvas.DrawSprite(HealthBarTexture, new Color(50, 50, 200, 250),
                new Rectangle((int)(center.X - canvas.Width/2 + 150),
                    (int)(center.Y - canvas.Height / 2 + 50),
                    (int)(MathHelper.Lerp(0, 500, player.InfectionPoints / player.MaxInfectionPoints)), 30),
                new Rectangle(0, 0, HealthBarTexture.Width, HealthBarTexture.Height));
            /*
            canvas.DrawText(player.InfectionPoints + "/" + player.MaxInfectionPoints, Color.White,
                new Vector2(player.Position.X - canvas.Width/2 + 10, player.Position.Y - canvas.Height / 2 + 50),
                "font1", false);
                */

            if (player.Items.Count > 0)
            {
                foreach (Item item in player.Items)
                {
                    switch (item.Type)
                    {
                        case ItemType.PLASMID:
                            PlayerMsg = "+Infection!";
                            break;
                        case ItemType.HEALTH:
                            PlayerMsg = "+Health!";
                            break;
                        case ItemType.ATTACK:
                            PlayerMsg = "Allies +Attack!";
                            break;
                        case ItemType.ALLIES:
                            PlayerMsg = "+" + GameUnitController.ITEM_FREE_ALLY_NUM + " Allies!";
                            break;
                        case ItemType.RANGE:
                            PlayerMsg = "+Range!";
                            break;
                        case ItemType.SPEED:
                            PlayerMsg = "+Speed!";
                            break;
                        case ItemType.MAX_HEALTH:
                            PlayerMsg = "+Max Health!";
                            break;
                        case ItemType.MAX_INFECT:
                            PlayerMsg = "+Max Infect!";
                            break;
                        case ItemType.INFECT_REGEN:
                            PlayerMsg = "+Infect Regen!";
                            break;
                        case ItemType.MYSTERY:
                            PlayerMsg = "+Mystery!";
                            break;
                        default:
                            PlayerMsg = "wat";
                            break;
                    }
                }
                stopwatch.Start();
            }

            if (PlayerMsg != null)
            {
                float time = stopwatch.ElapsedMilliseconds/(float)PLAYER_MSG_TIME;
                canvas.DrawText(PlayerMsg,
                    new Color(220, 200, 80) * (MathHelper.Lerp(250, 50, time)/250),
                    new Vector2(center.X, center.Y - MathHelper.Lerp(70, 120, time)),
                    "font2", true);
            }

            if (stopwatch.ElapsedMilliseconds >= PLAYER_MSG_TIME)
            {
                stopwatch.Stop();
                stopwatch.Reset();
                PlayerMsg = null;
            }

            // Draw minimap
            int[][] exploredTiles = player.ExploredTiles;
            int[][] mapTiles = level.Map.tiles;
            float px = center.X;
            float py = center.Y;
            canvas.DrawSprite(HealthBarTexture, new Color(0, 0, 0, 100),
                new Rectangle((int)px - MINIMAP_WIDTH/2 + 350, (int)py - MINIMAP_HEIGHT/2 - 300,
                    MINIMAP_WIDTH, MINIMAP_HEIGHT),
                new Rectangle(0, 0, HealthBarTexture.Width, HealthBarTexture.Height));

            for (int i = 0; i < exploredTiles.Length; i++)
            {
                for (int j = 0; j < exploredTiles[0].Length; j++)
                {
                    int x_trans = i - (int)px / Map.TILE_SIZE;
                    int y_trans = j - (int)py / Map.TILE_SIZE;

                    if(Math.Abs(x_trans) < MINIMAP_WIDTH/MINIMAP_TILE_SIZE/2 &&
                        Math.Abs(y_trans) < MINIMAP_HEIGHT/MINIMAP_TILE_SIZE/2) {
                        Color color = Color.Black;
                                
                        if (x_trans == 0 && y_trans == 0)
                        {
                            color = Color.Red;
                        }
                        else if (exploredTiles[j][i] == 1)
                        {
                            color = mapTiles[j][i] == 1 ? Color.Maroon : Color.LightPink;
                        }
                        canvas.DrawSprite(HealthBarTexture, color,
                            new Rectangle((int)(px + x_trans * MINIMAP_TILE_SIZE + 350), (int)(py + y_trans * MINIMAP_TILE_SIZE - 300),
                                MINIMAP_TILE_SIZE, MINIMAP_TILE_SIZE),
                            new Rectangle(0, 0, HealthBarTexture.Width, HealthBarTexture.Height));
                    }
                }
            }
        }

        // Player msg expiration event
        private void MsgExpire(object source, ElapsedEventArgs e)
        {

        }
    }
}
