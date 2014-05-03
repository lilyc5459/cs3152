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
using Pathogenesis.Controllers;

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
            private Dictionary<String, SoundEffect> music;
            // Dictionary of all audio clips mapped as <filename, clip>
            private Dictionary<String, SoundEffect> sound_effects;
            // Fonts mapped as <fontname, Spritefont>
            private Dictionary<String, SpriteFont> fonts;
            // Menu options
            private Dictionary<MenuType, Menu> menus;

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
            music = new Dictionary<string, SoundEffect>();
            sound_effects = new Dictionary<string, SoundEffect>();
            menus = new Dictionary<MenuType, Menu>();
            fonts = new Dictionary<string, SpriteFont>();
            levels = new List<Level>();
        }
        #endregion

        #region Content Loading
        // Loads all content from content directory
        public void LoadAllContent()
        {
            LoadContent();
            LoadConfigData();
            LoadLevels();
        }

        /*
         * Load all content assets
         */
        private void LoadContent()
        {
            try
            {
                // Load Textures
                String line;
                StreamReader sr = new StreamReader("Config/texture_paths.txt");
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("//")) continue;
                    String[] strings = line.Split(new char[] { ',' });
                    if (strings.Length < 2) continue;
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
                    if (strings.Length < 3) continue;
                    if(strings[1].Trim().Equals(SoundType.MUSIC.ToString())) {
                        music.Add(strings[0], content.Load<SoundEffect>(strings[2].Trim()));
                    }
                    else if(strings[1].Trim().Equals(SoundType.EFFECT.ToString()))
                    {
                        sound_effects.Add(strings[0], content.Load<SoundEffect>(strings[2].Trim()));
                    }
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
                    //menu_options.Add(strings[0].Trim(), strings[1].Trim().Split(new char[] { ',' }));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
        }

        /*
         * Load configuration data 
         * Animations, Menus
         */
        private void LoadConfigData()
        {
            // Load animation data
            XDocument animation_data = XDocument.Load("Config/animation_data.xml");
            foreach (XElement animation in animation_data.Descendants("Animations").Elements())
            {
                string name = animation.Element("Name").Value;
                int frame_w = int.Parse(animation.Element("FrameWidth").Value);
                int frame_h = int.Parse(animation.Element("FrameHeight").Value);
                int num_frames = int.Parse(animation.Element("NumFrames").Value);
                int frame_speed = int.Parse(animation.Element("FrameSpeed").Value);
                if (!animations.ContainsKey(name))
                {
                    animations.Add(name, new Dictionary<string, int>());
                }
                animations[name].Add("FrameWidth", frame_w);
                animations[name].Add("FrameHeight", frame_h);

                animations[name].Add("NumFrames", num_frames);
                animations[name].Add("FrameSpeed", frame_speed);
            }

            // Load menu data
            XDocument menu_data = XDocument.Load("Config/menu_config.xml");
            foreach (XElement menuElement in menu_data.Descendants("Menus").Elements())
            {
                try
                {
                    // Load type and title
                    MenuType type = (MenuType)Enum.Parse(typeof(MenuType), menuElement.Element("Type").Value);
                    string title = menuElement.Element("Title").Element("Text").Value;

                    // Load children
                    List<MenuType> children = new List<MenuType>();
                    foreach (XElement child in menuElement.Descendants("Child"))
                    {
                        children.Add((MenuType)Enum.Parse(typeof(MenuType), child.Value));
                    }

                    // Load options
                    List<MenuOption> options = AddMenuOptions(menuElement);

                    // Create menus
                    Menu menu = new Menu(type, options, children, textures["solid"]);
                    menus.Add(type, menu);
                }
                catch (ArgumentException)
                {
                    Console.WriteLine("Invalid MenuType while loading menu data");
                }
            }
        }

        // Recursively add menu options
        private List<MenuOption> AddMenuOptions(XElement root)
        {
            List<MenuOption> options = new List<MenuOption>();
            foreach (XElement option in root. Elements("Option"))
            {
                MenuOption opt = new MenuOption(option.Element("Text").Value,
                    new Vector2(
                        int.Parse(option.Element("XOffset").Value),
                        int.Parse(option.Element("YOffset").Value)),
                        AddMenuOptions(option));
                options.Add(opt);
            }
            return options;
        }

        /*
         * Load all levels into memory
         */
        private void LoadLevels()
        {

            // Load levels
            // TODO make config file for levels
            List<GameUnit> goals = new List<GameUnit>();
            //goals.Add(createUnit(UnitType.BOSS, UnitFaction.ENEMY, 1, new Vector2(500, 1800), false));
            //goals.Add(createUnit(UnitType.BOSS, UnitFaction.ENEMY, 1, new Vector2(1850, 1200), false));

            /*
            Level level = new Level(2000, 2000, textures["background"], textures["wall"], goals);
            level.PlayerStart = new Vector2(2, 2);
                */
            Level level = null;
            using (FileStream stream = new FileStream("regiontest.xml", FileMode.Open))
            {
                using (XmlReader reader = XmlReader.Create(stream))
                {
                    XmlSerializer x = new XmlSerializer(typeof(Level));
                    level = (Level)x.Deserialize(reader);
                }
            }
            //level.Organs.Add(createUnit(UnitType.ORGAN, UnitFaction.ENEMY, 1, new Vector2(1300, 1300), false));
            //level.Organs.Add(createUnit(UnitType.ORGAN, UnitFaction.ENEMY, 1, new Vector2(500, 500), false));

            level.BackgroundTexture = textures["background"];
            level.Map.WallTexture = textures["wall"];
            level.Bosses = goals;
            level.NumBosses = goals.Count;
            level.BossesDefeated = 0;
            level.PlayerStart = new Vector2(3, 3);

            SpawnPoint s = new SpawnPoint(new Vector2(15, 18), 1000);
            s.UnitProbabilities.Add(UnitType.TANK, 1);
            s.LevelProbabilities.Add(1, 1);
            level.Regions[1].SpawnPoints.Add(s);
            level.Regions[0].Center = new Vector2(5, 5);
            level.Regions[1].Center = new Vector2(13, 16);

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
                case UnitType.ORGAN:
                    unit = new GameUnit(textures["organ1"], type, faction, level, immune);
                    animation_data = animations["organ1"];
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
            if (typeProb > 0.2)
            {
                type = UnitType.TANK;
            }
            else
            {
                type = UnitType.FLYING;
            }
                /*
            else
            {
                type = UnitType.RANGED;
            }*/
            return createUnit(type, UnitFaction.ALLY, 1,
                pos + new Vector2((float)(rand.NextDouble() * 10 - 5), (float)(rand.NextDouble() * 10 - 5)), false);
        }

        /*
            * Creates an item of the specified type
            */
        public Item createItem(Vector2 pos, ItemType type)
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
        public Menu getMenu(MenuType type)
        {
            return menus[type];
        }

        /*
            * Create menus of all menu types
            */
        public Dictionary<MenuType, Menu> getMenus()
        {
            return menus;
        }
            
        /*
            * Returns the music map
            */
        public Dictionary<String, SoundEffect> getMusic()
        {
            return music;
        }

        /*
        * Returns the sound effects map
        */
        public Dictionary<String, SoundEffect> getSoundEffects()
        {
            return sound_effects;
        }


        /*
            * Returns the particle textures
            */
        public List<Texture2D> getParticleTextures()
        {
            return particle_textures;
        }

        /*
            * Returns the game font
            */
        public SpriteFont getFont(String name)
        {
            return fonts[name];
        }

        /*
            * Returns all of the fonts
            */
        public Dictionary<String, SpriteFont> getFonts()
        {
            return fonts;
        }
            
        /*
            * Returns the specified texture
            */
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
