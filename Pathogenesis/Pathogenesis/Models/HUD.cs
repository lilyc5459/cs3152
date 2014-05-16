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
        public const int FLASH_TIME = 100;
        public const int WASD_DELAY_TIME = 1000;
        public const int WASD_DISPLAY_TIME = 3000;
 
        public Texture2D InfectTexture { get; set; }
        public Texture2D HealthBarTexture { get; set; }
        public Texture2D ConversionTexture { get; set; }
        public Texture2D SpaceTexture { get; set; }
        public Texture2D WASDTexture { get; set; }

        public static Color HEALTH_COLOR = Color.Red;
        public static Color INFECT_COLOR = Color.Red;
        public static Color FONT_COLOR = Color.Wheat;
        public static Color FLASH_COLOR = Color.Red * 0.5f;

        public bool Active { get; set; }

        public List<String> PopMsg { get; set; }         // Floating msg indicator above player
        private List<int> finished_msgs;

        private List<Stopwatch> popmsg_stopwatches;
        private List<Stopwatch> finished_stopwatches;
        private Stopwatch flash_stopwatch;
        private Stopwatch glow_stopwatch;
        private Stopwatch tip_stopwatch;
        public static bool showWASD { get; set; }

        public HUD(Texture2D infect, Texture2D health, Texture2D conversion_texture,
            Texture2D space_texture, Texture2D wasd_texture)
        {
            InfectTexture = infect;
            HealthBarTexture = health;
            ConversionTexture = conversion_texture;
            SpaceTexture = space_texture;
            WASDTexture = wasd_texture;

            Active = true;

            PopMsg = new List<String>();
            finished_msgs = new List<int>();
            
            popmsg_stopwatches = new List<Stopwatch>();
            finished_stopwatches = new List<Stopwatch>();
            flash_stopwatch = new Stopwatch();
            glow_stopwatch = new Stopwatch();
            tip_stopwatch = new Stopwatch();

            showWASD = false;
        }

        public void Update(InputController input_controller)
        {
            if (input_controller.Toggle_HUD)
            {
                Active = !Active;
            }
        }

        public void DrawTutorial(GameCanvas canvas, List<GameUnit> units, Player player, Vector2 center)
        {
            if (player == null) return;
            float glowtime;
            if (glow_stopwatch.ElapsedMilliseconds < (float)PLAYER_MSG_TIME / 2)
            {
                glowtime = (float)glow_stopwatch.ElapsedMilliseconds / ((float)PLAYER_MSG_TIME / 2);
            }
            else
            {
                glowtime = 1 - ((float)glow_stopwatch.ElapsedMilliseconds - PLAYER_MSG_TIME / 2) / ((float)PLAYER_MSG_TIME / 2);
            }
            // Glow color
            Color color = Color.Lerp(Color.Wheat, new Color(220, 200, 130), glowtime);

            bool in_range = false;

            foreach (GameUnit unit in units)
            {
                if(unit.Exists && unit.Faction == UnitFaction.ENEMY && player.inRange(unit, player.InfectionRange))
                {
                    in_range = true;
                    canvas.DrawSprite(InfectTexture,
                        new Color(100, 100, 0, 100) * (MathHelper.Lerp(100, 250, glowtime) / 250),
                        new Rectangle((int)unit.Position.X - unit.Size / 2 - 10, (int)unit.Position.Y - unit.Size/2 - 10, unit.Size + 20, unit.Size + 20),
                        new Rectangle(0, 0, InfectTexture.Width, InfectTexture.Height));
                }
            }

            if (showWASD)
            {
                if (!tip_stopwatch.IsRunning)
                {
                    tip_stopwatch.Start();
                }
                if (!glow_stopwatch.IsRunning && tip_stopwatch.ElapsedMilliseconds >= WASD_DELAY_TIME)
                {
                    glow_stopwatch.Start();
                    tip_stopwatch.Reset();
                    tip_stopwatch.Start();
                }
                if (glow_stopwatch.IsRunning)
                {
                    // Draw WASD tip
                    canvas.DrawText("Move ",
                        color * (MathHelper.Lerp(120, 250, glowtime) / 250),
                        new Vector2(center.X - 140, center.Y + 200),
                        "font2", false);

                    canvas.DrawSprite(WASDTexture,
                        color * (MathHelper.Lerp(120, 250, glowtime) / 250),
                        new Rectangle((int)center.X + 20, (int)center.Y + 160, SpaceTexture.Width, SpaceTexture.Height),
                        new Rectangle(0, 0, SpaceTexture.Width, SpaceTexture.Height));

                    if (glow_stopwatch.ElapsedMilliseconds >= PLAYER_MSG_TIME)
                    {
                        glow_stopwatch.Reset();
                        glow_stopwatch.Start();
                    }
                    if (tip_stopwatch.ElapsedMilliseconds >= WASD_DISPLAY_TIME && glowtime < 0.1f)
                    {
                        showWASD = false;
                        glow_stopwatch.Stop();
                        glow_stopwatch.Reset();
                        tip_stopwatch.Stop();
                        tip_stopwatch.Reset();
                    }
                }
            }

            if (in_range)
            {
                // Draw infect tip
                canvas.DrawText("Infect ",
                    color * (MathHelper.Lerp(120, 250, glowtime) / 250),
                    new Vector2(center.X - 140, center.Y + 200),
                    "font2", false);

                canvas.DrawSprite(SpaceTexture,
                    color * (MathHelper.Lerp(120, 250, glowtime) / 250),
                    new Rectangle((int)center.X + 20, (int)center.Y + 160, SpaceTexture.Width, SpaceTexture.Height),
                    new Rectangle(0, 0, SpaceTexture.Width, SpaceTexture.Height));

                if (!glow_stopwatch.IsRunning) glow_stopwatch.Start();
                if (glow_stopwatch.ElapsedMilliseconds >= PLAYER_MSG_TIME)
                {
                    glow_stopwatch.Reset();
                    glow_stopwatch.Start();
                }
            }
            else if(!tip_stopwatch.IsRunning)
            {
                glow_stopwatch.Reset();
                glow_stopwatch.Stop();
            }
        }

        public void DrawLayerOne(GameCanvas canvas, List<GameUnit> units, Player player)
        {
            if (Active)
            {
                foreach (GameUnit unit in units)
                {
                    if (!unit.Exists) continue;
                    // Drop shadows
                    canvas.DrawSprite(InfectTexture,
                            new Color(0, 0, 0, 100),
                            new Rectangle((int)unit.Position.X - (unit.Size+10)/2 + 10, (int)unit.Position.Y + 10, unit.Size + 10, unit.Size/2),
                            new Rectangle(0, 0, InfectTexture.Width, InfectTexture.Height));
                }
                if (player != null)
                {
                    //Infection range
                    int range = player.InfectionRange;
                    canvas.DrawSprite(InfectTexture, new Color(24, 24, 0, 1),
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

                if (unit.Type == UnitType.TANK && unit.InfectionVitality < unit.max_infection_vitality)
                {
                    int frame = (int)MathHelper.Lerp(18, 0, unit.InfectionVitality / unit.max_infection_vitality);
                    canvas.DrawSprite(ConversionTexture, new Color(50, 100, 50) * 0.8f,
                        new Rectangle((int)unit.Position.X - 60 / 2, (int)unit.Position.Y - 70 / 2, 60, 70),
                        new Rectangle(frame * 83, 0, 83, 100));
                }

                //Infected bar indicator
                /*
                if ((unit.Type == UnitType.BOSS || unit.Type == UnitType.ORGAN) && unit.InfectionVitality != unit.max_infection_vitality)
                {
                    canvas.DrawSprite(HealthBarTexture, new Color(0, 50, 100, 200),
                                        new Rectangle((int)unit.Position.X - HealthBarTexture.Width / 2, (int)unit.Position.Y - 50, (int)MathHelper.Lerp(50, 0, unit.InfectionVitality/unit.max_infection_vitality), 8),
                                        new Rectangle(0, 0, HealthBarTexture.Width, (int)(HealthBarTexture.Height * 0.8)));
                }
                */
            }
            if (player == null) return;

            // Health
            if (player.Damaged)
            {
                flash_stopwatch.Start();
                player.Damaged = false;
            }
            if (flash_stopwatch.ElapsedMilliseconds >= PLAYER_MSG_TIME)
            {
                flash_stopwatch.Stop();
                flash_stopwatch.Reset();
            }
            if (flash_stopwatch.IsRunning)
            {
                float flash_time = flash_stopwatch.ElapsedMilliseconds / (float)FLASH_TIME;
                canvas.DrawSprite(HealthBarTexture, FLASH_COLOR * (1-flash_time),
                    new Rectangle((int)(center.X - canvas.Width / 2), (int)(center.Y - canvas.Height / 2),
                        canvas.Width, canvas.Height),
                    new Rectangle(0, 0, HealthBarTexture.Width, HealthBarTexture.Height));
            }

            canvas.DrawText("Health", FONT_COLOR,
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
            canvas.DrawText("Infect", FONT_COLOR,
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
                    Stopwatch new_stopwatch = new Stopwatch();
                    popmsg_stopwatches.Add(new_stopwatch);
                    switch (item.Type)
                    {
                        case ItemType.PLASMID:
                            PopMsg.Add("+Infection!");
                            break;
                        case ItemType.HEALTH:
                            PopMsg.Add("+Health!");
                            break;
                        case ItemType.ATTACK:
                            PopMsg.Add("Allies +Attack!");
                            break;
                        case ItemType.ALLIES:
                            PopMsg.Add("+" + GameUnitController.ITEM_FREE_ALLY_NUM + "Allies!");
                            break;
                        case ItemType.RANGE:
                            PopMsg.Add("+Range!");
                            break;
                        case ItemType.SPEED:
                            PopMsg.Add("+Speed!");
                            break;
                        case ItemType.MAX_HEALTH:
                            PopMsg.Add("+Max Health!");
                            break;
                        case ItemType.MAX_INFECT:
                            PopMsg.Add("+Max Infect!");
                            break;
                        case ItemType.INFECT_REGEN:
                            PopMsg.Add("+Infect Regen!");
                            break;
                        case ItemType.MYSTERY:
                            PopMsg.Add("+Mystery!");
                            break;
                        default:
                            PopMsg.Add("wat");
                            break;
                    }
                    new_stopwatch.Start();
                }
            }

            if (PopMsg.Count > 0)
            {
                for (int i = 0; i < PopMsg.Count; i++)
                {
                    float pop_time = popmsg_stopwatches[i].ElapsedMilliseconds / (float)PLAYER_MSG_TIME;
                    canvas.DrawText(PopMsg[i],
                        new Color(220, 200, 80) * (MathHelper.Lerp(250, 50, pop_time) / 250),
                        new Vector2(center.X, center.Y - MathHelper.Lerp(70, 120, pop_time)),
                        "font2", true);

                    if (popmsg_stopwatches[i].ElapsedMilliseconds >= PLAYER_MSG_TIME)
                    {
                        finished_stopwatches.Add(popmsg_stopwatches[i]);
                        finished_msgs.Add(i);
                    }
                }
            }

            foreach (Stopwatch s in finished_stopwatches)
            {
                popmsg_stopwatches.Remove(s);
            }
            finished_stopwatches.Clear();
            finished_msgs.Sort();
            finished_msgs.Reverse();
            for (int i = 0; i < finished_msgs.Count; i++)
            {
                PopMsg.RemoveAt(finished_msgs[i]);
            }
            finished_msgs.Clear();

            // Draw minimap
            int[][] exploredTiles = player.ExploredTiles;
            int[][] mapTiles = level.Map.tiles;
            float px = center.X;
            float py = center.Y;
            canvas.DrawSprite(HealthBarTexture, new Color(0, 0, 0, 100),
                new Rectangle((int)px - MINIMAP_WIDTH/2 + 350, (int)py - MINIMAP_HEIGHT/2 - 300,
                    MINIMAP_WIDTH, MINIMAP_HEIGHT),
                new Rectangle(0, 0, HealthBarTexture.Width, HealthBarTexture.Height));

            for (int i = 0; i < exploredTiles[0].Length; i++)
            {
                for (int j = 0; j < exploredTiles.Length; j++)
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
    }
}
