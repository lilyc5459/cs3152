using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
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
            // Particle textures
            private List<Texture2D> particle_textures;
            // Animation data mapped as <name, <animation_attribute, value>>
            private Dictionary<String, Dictionary<String, int>> animations;
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
                particle_textures = new List<Texture2D>();
                animations = new Dictionary<string, Dictionary<string, int>>();
                sounds = new Dictionary<string, SoundEffect>();
                menu_options = new Dictionary<string, string[]>();
                fonts = new Dictionary<string, SpriteFont>();
                levels = new List<Level>();
            }
        #endregion

        #region Content Loading
            // Loads all content from content directory
            public void LoadAllContent()
            {
                // Load animation data
                XDocument animation_data = XDocument.Load("Config/animation_data.xml");
                foreach(XElement animation in animation_data.Descendants("Animations").Elements()) {
                    string name = animation.Element("Name").Value;
                    int frame_w = int.Parse(animation.Element("FrameWidth").Value);
                    int frame_h = int.Parse(animation.Element("FrameHeight").Value);
                    int num_frames = int.Parse(animation.Element("NumFrames").Value);
                    int frame_speed = int.Parse(animation.Element("FrameSpeed").Value);
                    if(!animations.ContainsKey(name))
                    {
                        animations.Add(name, new Dictionary<string, int>());
                    }
                    animations[name].Add("FrameWidth", frame_w);
                    animations[name].Add("FrameHeight", frame_h);

                    animations[name].Add("NumFrames", num_frames);
                    animations[name].Add("FrameSpeed", frame_speed);
                }

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
                    // Set particle textures
                    particle_textures.Add(textures["circle"]);

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

                // Load levels
                // TODO make config file for levels
                List<GameUnit> goals = new List<GameUnit>();
                goals.Add(createUnit(UnitType.BOSS, UnitFaction.ENEMY, 1, new Vector2(500, 1800), false));
                goals.Add(createUnit(UnitType.BOSS, UnitFaction.ENEMY, 1, new Vector2(1850, 1200), false));
                /*
                Level level = new Level(2000, 2000, textures["background"], textures["wall"], goals);
                level.PlayerStart = new Vector2(2, 2);
                 */
                Level level = null;
                using (FileStream stream = new FileStream("level.xml", FileMode.Open))
                {
                    using (XmlReader reader = XmlReader.Create(stream))
                    {
                        XmlSerializer x = new XmlSerializer(typeof(Level));
                        level = (Level)x.Deserialize(reader);
                    }
                }

                level.BackgroundTexture = textures["background"];
                level.Map.WallTexture = textures["wall"];
                level.Bosses = goals;
                level.NumBosses = goals.Count;
                level.BossesDefeated = 0;
                level.PlayerStart = new Vector2(3, 3);

                levels.Add(level);
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
                Player p = new Player(textures["player_sheet"]);
                Dictionary<string, int> player_ani = animations["player"];
                p.NumFrames = player_ani["NumFrames"];
                p.FrameSize = new Vector2(player_ani["FrameWidth"], player_ani["FrameHeight"]);
                p.FrameSpeed = player_ani["FrameSpeed"];
                p.Position = pos;
                return p;
            }

            public HUD createHUD(Player player)
            {
                return new HUD(textures["infect_range"], textures["health_bar"]);
            }
        
            // Returns an instance of a unit of the given type and faction
            public GameUnit createUnit(UnitType type, UnitFaction faction, int level, Vector2 pos, bool immune)
            {
                GameUnit unit = null;
                Dictionary<string, int> animation_data = null;
                switch (type)
                {
                    case UnitType.TANK:
                        switch (level)
                        {
                            case 1:
                                if (faction == UnitFaction.ALLY)
                                {
                                    unit = new GameUnit(textures["ally1_sheet"], type, faction, level, immune);
                                }
                                else
                                {
                                    unit = new GameUnit(textures["enemy1_sheet"], type, faction, level, immune);
                                }
                                animation_data = animations["tank1"];
                                break;
                            case 2:
                                if (faction == UnitFaction.ALLY)
                                {
                                    unit = new GameUnit(textures["ally2_sheet"], type, faction, level, immune);
                                }
                                else
                                {
                                    unit = new GameUnit(textures["enemy2_sheet"], type, faction, level, immune);
                                }
                                animation_data = animations["tank2"];
                                break;
                            case 3:
                                if (faction == UnitFaction.ALLY)
                                {
                                    unit = new GameUnit(textures["ally2_sheet"], type, faction, level, immune);
                                }
                                else
                                {
                                    unit = new GameUnit(textures["enemy2_sheet"], type, faction, level, immune);
                                }
                                animation_data = animations["tank2"];
                                break;
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
                        animation_data = animations["ranged1"];
                         * */
                        goto case UnitType.FLYING;
                        break;
                    case UnitType.FLYING:
                        if (faction == UnitFaction.ALLY)
                        {
                            unit = new GameUnit(textures["ally3_sheet"], type, faction, level, immune);
                        }
                        else
                        {
                            unit = new GameUnit(textures["enemy3_sheet"], type, faction, level, immune);
                        }
                        animation_data = animations["flying1"];
                        break;
                    case UnitType.BOSS:
                        unit = new GameUnit(textures["boss1"], type, faction, level, immune);
                        animation_data = animations["boss1"];
                        break;
                    default:
                        unit = null;
                        break;
                }
                if (unit != null)
                {
                    unit.Position = pos;
                    if (animation_data != null)
                    {
                        unit.FrameSize = new Vector2(animation_data["FrameWidth"], animation_data["FrameHeight"]);
                        unit.NumFrames = animation_data["NumFrames"];
                        unit.FrameSpeed = animation_data["FrameSpeed"];
                    }
                }
                return unit;
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
                Item item = null;
                switch (type)
                {
                    case ItemType.PLASMID:
                        item = new Item(textures["plasmid"], type);
                        break;
                    case ItemType.HEALTH:
                        item = new Item(textures["health"], type);
                        break;
                    case ItemType.ALLIES:
                        item = new Item(textures["health"], type);
                        break;
                    default:
                        break;
                }
                if (item != null)
                {
                    item.Position = pos;
                }
                return item;
            }

            /*
             * Retreive the specified level
             */
            public Level loadLevel(int num)
            {
                if (levels.Count > num)
                {
                    return levels[num];
                }
                else
                {
                    return levels[levels.Count-1];
                }
            }

            /*
             * Create a menu of the specified type
             */
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

            public List<Texture2D> getParticleTextures()
            {
                return particle_textures;
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

            public Texture2D getTexture(String name)
            {
                if (textures.ContainsKey(name))
                {
                    return textures[name];
                }
                return null;
            }
        #endregion
    }
}
