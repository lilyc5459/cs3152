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

            // Content directories and filenames
            // Textures
            private const string CHARACTERS_DIR = "Characters/";
            private const string BACKGROUNDS_DIR = "Backgrounds/";
            private const string ENVIRONMENT_DIR = "Environment/";
            private const string SOUNDS_DIR = "Sounds/";

            private const string SOLID = ENVIRONMENT_DIR + "solid";

            private const string MAINPLAYER_r = CHARACTERS_DIR + "main-right";
            private const string MAINPLAYER_l = CHARACTERS_DIR + "main-left";
            private const string MAINPLAYER_u = CHARACTERS_DIR + "main-back";
            private const string MAINPLAYER_d = CHARACTERS_DIR + "main-front";
            private const string INFECT_RANGE = CHARACTERS_DIR + "infect";
            private const string HEALTH_BAR = CHARACTERS_DIR + "healthbar";

            private const string ENEMY_TANK_r = CHARACTERS_DIR + "enemy1-right";
            private const string ENEMY_TANK_l = CHARACTERS_DIR + "enemy1-left";
            private const string ENEMY_TANK_u = CHARACTERS_DIR + "enemy1-back";
            private const string ENEMY_TANK_d = CHARACTERS_DIR + "enemy1-front";
            private const string ENEMY_RANGED = CHARACTERS_DIR + "enemy_ranged";
            private const string ENEMY_FLYING_r = CHARACTERS_DIR + "enemy2-right";
            private const string ENEMY_FLYING_l = CHARACTERS_DIR + "enemy2-left";
            private const string ENEMY_FLYING_u = CHARACTERS_DIR + "enemy2-back";
            private const string ENEMY_FLYING_d = CHARACTERS_DIR + "enemy2-front";

            private const string ALLY_TANK_r = CHARACTERS_DIR + "ally1-right";
            private const string ALLY_TANK_l = CHARACTERS_DIR + "ally1-left";
            private const string ALLY_TANK_u = CHARACTERS_DIR + "ally1-back";
            private const string ALLY_TANK_d = CHARACTERS_DIR + "ally1-front";
            private const string ALLY_RANGED = CHARACTERS_DIR + "ally_ranged";
            private const string ALLY_FLYING_r = CHARACTERS_DIR + "ally2-right";
            private const string ALLY_FLYING_l = CHARACTERS_DIR + "ally2-left";
            private const string ALLY_FLYING_u = CHARACTERS_DIR + "ally2-back";
            private const string ALLY_FLYING_d = CHARACTERS_DIR + "ally2-front";

            private const string PLASMID = CHARACTERS_DIR + "plasmid";

            private const string WALL = BACKGROUNDS_DIR + "wall_square";
            private const string BACKGROUND1 = BACKGROUNDS_DIR + "background_long2";
            private const string BACKGROUND2 = BACKGROUNDS_DIR + "background2";
            private const string BACKGROUND3 = BACKGROUNDS_DIR + "background3";

            // Sounds
            private const string MUSIC1 = SOUNDS_DIR + "music1";
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
                // Load textures into the textures map
                textures.Add(SOLID, content.Load<Texture2D>(SOLID));

                textures.Add(MAINPLAYER_r, content.Load<Texture2D>(MAINPLAYER_r));
                textures.Add(MAINPLAYER_l, content.Load<Texture2D>(MAINPLAYER_l));
                textures.Add(MAINPLAYER_u, content.Load<Texture2D>(MAINPLAYER_u));
                textures.Add(MAINPLAYER_d, content.Load<Texture2D>(MAINPLAYER_d));
                textures.Add(INFECT_RANGE, content.Load<Texture2D>(INFECT_RANGE));
                textures.Add(HEALTH_BAR, content.Load<Texture2D>(HEALTH_BAR));

                textures.Add(ENEMY_TANK_r, content.Load<Texture2D>(ENEMY_TANK_r));
                textures.Add(ENEMY_TANK_l, content.Load<Texture2D>(ENEMY_TANK_l));
                textures.Add(ENEMY_TANK_u, content.Load<Texture2D>(ENEMY_TANK_u));
                textures.Add(ENEMY_TANK_d, content.Load<Texture2D>(ENEMY_TANK_d));
                textures.Add(ENEMY_RANGED, content.Load<Texture2D>(ENEMY_RANGED));
                textures.Add(ENEMY_FLYING_r, content.Load<Texture2D>(ENEMY_FLYING_r));
                textures.Add(ENEMY_FLYING_l, content.Load<Texture2D>(ENEMY_FLYING_l));
                textures.Add(ENEMY_FLYING_u, content.Load<Texture2D>(ENEMY_FLYING_u));
                textures.Add(ENEMY_FLYING_d, content.Load<Texture2D>(ENEMY_FLYING_d));

                textures.Add(ALLY_TANK_r, content.Load<Texture2D>(ALLY_TANK_r));
                textures.Add(ALLY_TANK_l, content.Load<Texture2D>(ALLY_TANK_l));
                textures.Add(ALLY_TANK_u, content.Load<Texture2D>(ALLY_TANK_u));
                textures.Add(ALLY_TANK_d, content.Load<Texture2D>(ALLY_TANK_d));
                textures.Add(ALLY_RANGED, content.Load<Texture2D>(ALLY_RANGED));
                textures.Add(ALLY_FLYING_r, content.Load<Texture2D>(ALLY_FLYING_r));
                textures.Add(ALLY_FLYING_l, content.Load<Texture2D>(ALLY_FLYING_l));
                textures.Add(ALLY_FLYING_u, content.Load<Texture2D>(ALLY_FLYING_u));
                textures.Add(ALLY_FLYING_d, content.Load<Texture2D>(ALLY_FLYING_d));

                textures.Add(PLASMID, content.Load<Texture2D>(PLASMID));

                textures.Add(WALL, content.Load<Texture2D>(WALL));
                textures.Add(BACKGROUND1, content.Load<Texture2D>(BACKGROUND1));
                textures.Add(BACKGROUND2, content.Load<Texture2D>(BACKGROUND2));
                textures.Add(BACKGROUND3, content.Load<Texture2D>(BACKGROUND3));

                // Loads sounds into the audio clip map
                sounds.Add(MUSIC1, content.Load<SoundEffect>(MUSIC1));

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
                Player p = new Player(textures[MAINPLAYER_l], textures[MAINPLAYER_r],
                    textures[MAINPLAYER_u], textures[MAINPLAYER_d]);
                p.Position = pos;
                return p;
            }

            public HUD createHUD(Player player)
            {
                return new HUD(textures[INFECT_RANGE], textures[HEALTH_BAR]);
            }
        
            // Returns an instance of a unit of the given type and faction
            public GameUnit createUnit(UnitType type, UnitFaction faction, int level, Vector2 pos, bool immune)
            {
                GameUnit enemy;
                switch (type)
                {
                    case UnitType.TANK:
                        if (faction == UnitFaction.ALLY)
                        {
                            enemy = new GameUnit(textures[ALLY_TANK_l], textures[ALLY_TANK_r],
                                textures[ALLY_TANK_r], textures[ALLY_TANK_r], type, faction, level, immune);
                        }
                        else
                        {
                            enemy = new GameUnit(textures[ENEMY_TANK_l], textures[ENEMY_TANK_r],
                                textures[ENEMY_TANK_u], textures[ENEMY_TANK_d], type, faction, level, immune);
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
                        enemy = null;
                        break;
                    case UnitType.FLYING:
                        if (faction == UnitFaction.ALLY)
                        {
                            enemy = new GameUnit(textures[ALLY_FLYING_l], textures[ALLY_FLYING_r],
                                textures[ALLY_FLYING_u], textures[ALLY_FLYING_d], type, faction, level, immune);
                        }
                        else
                        {
                            enemy = new GameUnit(textures[ENEMY_FLYING_l], textures[ENEMY_FLYING_r],
                                textures[ENEMY_FLYING_u], textures[ENEMY_FLYING_d], type, faction, level, immune);
                        }
                        break;
                    case UnitType.BOSS:
                        enemy = new GameUnit(textures[PLASMID], type, faction, level, immune);
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
                Item item = new Item(textures[PLASMID], type);
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
                goals.Add(createUnit(UnitType.BOSS, UnitFaction.ENEMY, 1, new Vector2(800, 800), false));
                goals.Add(createUnit(UnitType.BOSS, UnitFaction.ENEMY, 1, new Vector2(1000, 200), false));
                Level level = new Level(2000, 2000, textures[BACKGROUND1], textures[WALL], goals);
                level.PlayerStart = new Vector2(2, 2);
                return level;
            }

            public Menu createMenu(MenuType type)
            {
                return new Menu(type, textures[SOLID]);
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
