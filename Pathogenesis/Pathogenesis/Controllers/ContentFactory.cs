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

            // Dictionary of all textures mapped as <filename, texture>
            private Dictionary<String, Texture2D> textures;
            // Dictionary of all audio clips mapped as <filename, clip>
            private Dictionary<String, SoundEffect> sounds;

            // List of levels in the order they appear in the game
            private List<Level> levels;

            protected SpriteFont font;
        #endregion

        #region Initialization
            public ContentFactory(ContentManager content)
            {
                // Tell the program to load all files relative to the "Content" directory.
                this.content = content;
                content.RootDirectory = "Content";

                textures = new Dictionary<string, Texture2D>();
                sounds = new Dictionary<string, SoundEffect>();
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
                }
                catch (Exception e)
                {
                    Console.WriteLine("The file could not be read:");
                    Console.WriteLine(e.Message);
                }

                // Load fonts
                font = content.Load<SpriteFont>("Fonts/font");
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
                Player p = new Player(textures["player_left"], textures["player_right"],
                    textures["player_back"], textures["player_front"], 2, new Vector2(60, 47));
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

                // TODO Should all be loaded from file
                
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
                return new Menu(type, textures["solid"]);
            }

            public Dictionary<String, SoundEffect> getSounds()
            {
                return sounds;
            }

            // Returns the game font
            public SpriteFont getFont()
            {
                return font;
            }

        #endregion
    }
}
