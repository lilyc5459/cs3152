using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Pathogenesis.Models;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Pathogenesis
{
    public class ContentFactory
    {
        #region Fields
            // Used to load the sounds and graphics (CONTROLLER CLASS)
            protected ContentManager content;

            // Random num generator
            private Random rand;

            // Dictionary of all textures mapped as <filename, texture>
            private Dictionary<String, Texture2D> textures;
            // Dictionary of all audio clips mapped as <filename, clip>
            private Dictionary<String, SoundEffect> sounds;
            // Fonts mapped as <fontname, Spritefont>
            private Dictionary<String, SpriteFont> fonts;
            // Menu options
            private Dictionary<String, String[]> menu_options;

            // List of levels in the order they appear in the game
            private List<Level> levels;
        #endregion

        #region Initialization
            public ContentFactory(ContentManager content)
            {
                // Tell the program to load all files relative to the "Content" directory.
                this.content = content;
                content.RootDirectory = "Content";

                rand = new Random();

                textures = new Dictionary<string, Texture2D>();
                sounds = new Dictionary<string, SoundEffect>();
                menu_options = new Dictionary<string, string[]>();
                fonts = new Dictionary<string, SpriteFont>();
                levels = new List<Level>();
            }

            // Loads all content from content directory
            public void LoadAllContent()
            {
                try
                {
                    // Load Textures
                    String line;
                    StreamReader sr = new StreamReader("Config/texture_paths.txt");
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.StartsWith("//")) continue;
                        String[] strings = line.Split(new char[] {','});
                        if(strings.Length < 2) continue;
                        textures.Add(strings[0], content.Load<Texture2D>(strings[1].Trim()));
                    }

                    // Load Sounds
                    sr = new StreamReader("Config/sound_paths.txt");
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.StartsWith("//")) continue;
                        String[] strings = line.Split(new char[] { ',' });
                        if (strings.Length < 2) continue;
                        sounds.Add(strings[0], content.Load<SoundEffect>(strings[1].Trim()));
                    }

                    // Load fonts
                    sr = new StreamReader("Config/font_paths.txt");
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.StartsWith("//")) continue;
                        String[] strings = line.Split(new char[] { ',' });
                        if (strings.Length < 2) continue;
                        fonts.Add(strings[0].Trim(), content.Load<SpriteFont>(strings[1].Trim()));
                    }

                    // Setup menus
                    sr = new StreamReader("Config/menu_config.txt");
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.StartsWith("//")) continue;
                        String[] strings = line.Split(new char[] { ';' });
                        if (strings.Length < 2) continue;
                        menu_options.Add(strings[0].Trim(), strings[1].Trim().Split(new char[] { ',' }));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("The file could not be read:");
                    Console.WriteLine(e.Message);
                }
            }

            public void UnloadAll()
            {
                content.Unload();
            }
        #endregion

        #region Instantiation functions
            // Returns an instance of Player
            public Player createPlayer(Vector2 pos)
            {
                Player p = new Player(textures["player_sheet"], textures["player_right"],
                    textures["player_back"], textures["player_front"], 3, new Vector2(60, 47));
                p.Position = pos;
                p.FrameSpeed = 9;
                return p;
            }

            public HUD createHUD(Player player)
            {
                return new HUD(textures["infect_range"], textures["health_bar"]);
            }
        
            // Returns an instance of a unit of the given type and faction
            public GameUnit createUnit(UnitType type, UnitFaction faction, int level, Vector2 pos, bool immune)
            {
                GameUnit enemy = null;
                switch (type)
                {
                    case UnitType.TANK:
                        if (faction == UnitFaction.ALLY)
                        {
                            switch(level) {
                                case 1: 
                                    enemy = new GameUnit(textures["ally1_left"], textures["ally1_right"],
                                        textures["ally1_back"], textures["ally1_front"], 1, new Vector2(), type, faction, level, immune);
                                    break;
                                case 2:
                                    enemy = new GameUnit(textures["ally2_left"], textures["ally2_right"],
                                        textures["ally2_back"], textures["ally2_front"], 1, new Vector2(), type, faction, level, immune);
                                    break;
                                case 3:
                                    enemy = new GameUnit(textures["ally2_left"], textures["ally2_right"],
                                        textures["ally2_back"], textures["ally2_front"], 1, new Vector2(), type, faction, level, immune);
                                    break;
                            }
                        }
                        else
                        {
                            switch (level)
                            {
                                case 1:
                                    enemy = new GameUnit(textures["enemy1_left"], textures["enemy1_right"],
                                        textures["enemy1_back"], textures["enemy1_front"], 1, new Vector2(), type, faction, level, immune);
                                    break;
                                case 2:
                                    enemy = new GameUnit(textures["enemy2_left"], textures["enemy2_right"],
                                        textures["enemy2_back"], textures["enemy2_front"], 1, new Vector2(), type, faction, level, immune);
                                    break;
                                case 3:
                                    enemy = new GameUnit(textures["enemy2_left"], textures["enemy2_right"],
                                        textures["enemy2_back"], textures["enemy2_front"], 1, new Vector2(), type, faction, level, immune);
                                    break;
                            }
                        }
                        break;
                    case UnitType.RANGED:
                        /*
                        if (faction == UnitFaction.ALLY)
                        {
                            enemy = new GameUnit(textures[ALLY_RANGED_l], textures[ALLY_RANGED_r],
                                textures[ALLY_RANGED_r], textures[ALLY_RANGED_r], type, faction, level, immune);
                        }
                        else
                        {
                            enemy = new GameUnit(textures[ENEMY_RANGED_l], textures[ENEMY_RANGED_r],
                                textures[ENEMY_RANGED_r], textures[ENEMY_RANGED_r], type, faction, level, immune);
                        }
                         * */
                        goto case UnitType.FLYING;
                        break;
                    case UnitType.FLYING:
                        if (faction == UnitFaction.ALLY)
                        {
                            enemy = new GameUnit(textures["ally3_left"], textures["ally3_right"],
                                textures["ally3_back"], textures["ally3_front"], 1, new Vector2(), type, faction, level, immune);
                        }
                        else
                        {
                            enemy = new GameUnit(textures["enemy3_left"], textures["enemy3_right"],
                                textures["enemy3_back"], textures["enemy3_front"], 1, new Vector2(), type, faction, level, immune);
                        }
                        break;
                    case UnitType.BOSS:
                        enemy = new GameUnit(textures["boss1"], 1, new Vector2(), type, faction, level, immune);
                        break;
                    default:
                        enemy = null;
                        break;
                }
                if (enemy != null)
                {
                    enemy.Position = pos;
                }
                return enemy;
            }

            /*
             * Creates a base level ally
             */
            public GameUnit createAlly(Vector2 pos)
            {
                //TODO don't hardcode probabilities
                double typeProb = rand.NextDouble();
                UnitType type = UnitType.TANK;
                if (typeProb > 0.4)
                {
                    type = UnitType.TANK;
                }
                else if (typeProb > 0.1)
                {
                    type = UnitType.FLYING;
                }
                else
                {
                    type = UnitType.RANGED;
                }
                return createUnit(type, UnitFaction.ALLY, 1,
                    pos + new Vector2((float)(rand.NextDouble() * 10 - 5), (float)(rand.NextDouble() * 10 - 5)), false);
            }

            public Item createPickup(Vector2 pos, ItemType type)
            {
                Item item = new Item(textures["plasmid"], type);
                if (item != null)
                {
                    item.Position = pos;
                }
                return item;
            }

            public Level loadLevel(int num)
            {
                //return levels[num];

                List<GameUnit> goals = new List<GameUnit>();
                goals.Add(createUnit(UnitType.BOSS, UnitFaction.ENEMY, 1, new Vector2(500, 1800), false));
                goals.Add(createUnit(UnitType.BOSS, UnitFaction.ENEMY, 1, new Vector2(1850, 1200), false));
                /*
                Level level = new Level(2000, 2000, textures["background"], textures["wall"], goals);
                level.PlayerStart = new Vector2(2, 2);
                 */
                //return level;

                //load from memory
                Level loadedLevel = null;

                using (FileStream stream = new FileStream("level.xml", FileMode.Open))
                {
                    using (XmlReader reader = XmlReader.Create(stream))
                    {
                        XmlSerializer x = new XmlSerializer(typeof(Level));
                        loadedLevel = (Level)x.Deserialize(reader);
                    }
                }

                loadedLevel.BackgroundTexture = textures["background"];
                loadedLevel.Map.WallTexture = textures["wall"];
                loadedLevel.Bosses = goals;
                loadedLevel.NumBosses = goals.Count;
                loadedLevel.BossesDefeated = 0;
                loadedLevel.PlayerStart = new Vector2(3, 3);

                return loadedLevel;
            }

            public Menu createMenu(MenuType type)
            {
                return new Menu(type, menu_options[type.ToString()], textures["solid"]);
            }

            /*
             * Create menus of all menu types
             */
            public Dictionary<MenuType, Menu> createMenus()
            {
                Dictionary<MenuType, Menu> menus = new Dictionary<MenuType, Menu>();
                foreach (MenuType type in Enum.GetValues(typeof(MenuType)))
                {
                    menus.Add(type, new Menu(type, menu_options[type.ToString()], textures["solid"]));
                }
                return menus;
            }

            public Dictionary<String, SoundEffect> getSounds()
            {
                return sounds;
            }

            // Returns the game font
            public SpriteFont getFont(String name)
            {
                return fonts[name];
            }

            public Dictionary<String, SpriteFont> getFonts()
            {
                return fonts;
            }

        #endregion
    }
}
