using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Pathogenesis.Models;
using Pathogenesis.Controllers;

namespace Pathogenesis
{
    #region Enum
    /*
     * Possible game states
     */ 
    public enum GameState
    {
        MAIN_MENU,  // Player is at the main menu
        IN_GAME,    // Player is playing the game
        VICTORY,    // Player has completed the level
        LOSE,       // Player has died
        PAUSED      // Player has activated the pause menu
    }
    #endregion

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class GameEngine : Microsoft.Xna.Framework.Game
    {
        #region Fields
        // First time game loaded?
        private bool firstLoop = true;

        private SpriteBatch spriteBatch;

        // Used to draw the game onto the screen (VIEW CLASS)
        private GameCanvas canvas;

        // Used to load content and create game objects
        private ContentFactory factory;

        // Game camera, position determines portion of map drawn on screen
        private Camera camera;

        // Fades game to state
        private Fader fader;

        // Game operation controllers
        private InputController input_controller;
        private SoundController sound_controller;
        private CollisionController collision_controller;
        private ParticleEngine particle_engine;

        // Game entity controllers
        private GameUnitController unit_controller;
        private ItemController item_controller;
        private LevelController level_controller;
        private MenuController menu_controller;

        private HUD HUD_display;
        private GameState game_state;

        private Texture2D solid;
        #endregion

        #region Initialization
        public GameEngine()
        {
            canvas = new GameCanvas(this);
            factory = new ContentFactory(new ContentManager(Services));
            camera = new Camera(canvas.Width, canvas.Height);
            fader = new Fader();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            if (firstLoop) factory.LoadAllContent();
            canvas.Initialize(this);
            solid = factory.getTexture("solid");

            // Initialize controllers
            input_controller = new InputController();
            sound_controller = new SoundController(factory);
            collision_controller = new CollisionController();
            particle_engine = new ParticleEngine(factory.getParticleTextures());

            level_controller = new LevelController();
            item_controller = new ItemController(factory);
            unit_controller = new GameUnitController(factory, particle_engine, item_controller);
            menu_controller = new MenuController(factory, sound_controller);

            // Game starts at the main menu
            game_state = GameState.MAIN_MENU;
            menu_controller.LoadMenu(MenuType.MAIN);

            // TEST
            HUD_display = factory.createHUD(unit_controller.Player);

            base.Initialize();
            firstLoop = false;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            canvas.LoadContent(factory);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            //canvas.Dispose();
            factory.UnloadAll();
        }
    #endregion

        #region Update Functions
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            fader.Update();
            input_controller.Update(fader.Fading);    // Receive and process input
            sound_controller.Update();

            switch (game_state)
            {
                case GameState.IN_GAME:
                    Random rand = new Random();

                    item_controller.Update();

                    // Move later
                    particle_engine.EmitterPosition = camera.Position;
                    if (unit_controller.Player.Infecting != null)
                    {
                        particle_engine.GenerateParticle(1, Color.Black, unit_controller.Player.Position,
                            unit_controller.Player.Infecting, true);
                    }
                    particle_engine.UpdateParticles();

                    // Process level environment logic
                    bool victory = level_controller.Update();

                    // Remove later
                    if (rand.NextDouble() < 0.02 && unit_controller.Units.Count < 80)
                    {
                        Vector2 pos = new Vector2(rand.Next(level_controller.CurLevel.Width), rand.Next(level_controller.CurLevel.Height));
                        if (level_controller.CurLevel.Map.canMoveToWorldPos(pos))
                        {
                            int level = rand.NextDouble() < 0.2 ? (rand.NextDouble() < 0.2 ? 2 : 2) : 1;
                            unit_controller.AddUnit(factory.createUnit(rand.NextDouble() < 0.5 ? UnitType.FLYING : UnitType.TANK, UnitFaction.ENEMY, level,
                                pos,
                                rand.NextDouble() < 0.3 ? true : false));

                            if (rand.NextDouble() < 0.2)
                            {
                                item_controller.AddItem(factory.createItem(pos,
                                    rand.NextDouble() < 0.5 ? ItemType.PLASMID : rand.NextDouble() < 0.1 ? ItemType.ALLIES : ItemType.HEALTH));
                            }
                        }   
                    }

                    //
                    /* TODO: Remove DEBUG CODE HERE (remove later)
                     * 
                     */
                    if (input_controller.Spawn_Enemy)
                    {
                        unit_controller.AddUnit(factory.createUnit(UnitType.TANK, UnitFaction.ENEMY, 1,
                            new Vector2(rand.Next(level_controller.CurLevel.Width), rand.Next(level_controller.CurLevel.Height)), false));
                    }
                    if (input_controller.Spawn_Ally && unit_controller.Player != null)
                    {
                        unit_controller.AddUnit(factory.createUnit(UnitType.TANK, UnitFaction.ALLY, 1,
                            unit_controller.Player.Position + new Vector2(1, 1), false));
                    }
                    if (input_controller.Spawn_Plasmid)
                    {
                        item_controller.AddItem(factory.createItem(new Vector2(rand.Next(level_controller.CurLevel.Width), rand.Next(level_controller.CurLevel.Height)),
                            ItemType.PLASMID));
                    }
                    //Auto win
                    if (input_controller.Enter)
                    {
                        victory = true;
                    }
                    //**/

                    //Restart
                    if (input_controller.Restart)
                    {
                        level_controller.Restart(factory, unit_controller, item_controller, sound_controller);
                    }

                    if (input_controller.Pause)
                    {
                        game_state = GameState.PAUSED;
                        menu_controller.LoadMenu(MenuType.PAUSE);
                    }

                    // Process and update all units
                    unit_controller.Update(level_controller.CurLevel, input_controller);

                    // Process and handle collisions
                    collision_controller.Update(unit_controller.Units, unit_controller.Player, unit_controller.PreviousPositions,
                        level_controller.CurLevel, item_controller);

                    // Update the HUD
                    HUD_display.Update(input_controller);
                    
                    if (victory)
                    {
                        game_state = GameState.VICTORY;
                        menu_controller.LoadMenu(MenuType.WIN);
                    }
                    if (unit_controller.Player == null)
                    {
                        game_state = GameState.LOSE;
                        menu_controller.LoadMenu(MenuType.LOSE);
                    }
                    else
                    {
                        camera.Position = unit_controller.Player.Position;
                    }
                    break;
                case GameState.MAIN_MENU:
                    menu_controller.HandleMenuInput(this, input_controller);
                    break;
                case GameState.PAUSED:
                    menu_controller.HandleMenuInput(this, input_controller);
                    break;
                case GameState.VICTORY:
                    menu_controller.HandleMenuInput(this, input_controller);
                    break;
                case GameState.LOSE:
                    menu_controller.HandleMenuInput(this, input_controller);
                    break;
            }

            base.Update(gameTime);
        }

        /*
         * Fade to the specified game state
         */
        public void fadeTo(GameState state)
        {
            fader.startFade(ChangeGameState, state);
        }

        /*
         * Apply a gamestate change
         */
        public void ChangeGameState(GameState state)
        {
            particle_engine.Reset();
            switch (state)
            {
                case GameState.IN_GAME:
                    if (game_state == GameState.MAIN_MENU)
                    {
                        level_controller.LoadLevel(factory, unit_controller, item_controller, sound_controller, 0);
                    }
                    else if (game_state == GameState.VICTORY)
                    {
                        level_controller.NextLevel(factory, unit_controller, item_controller, sound_controller);
                    }
                    else if (game_state == GameState.LOSE)
                    {
                        level_controller.Restart(factory, unit_controller, item_controller, sound_controller);
                    }
                    break;
                case GameState.MAIN_MENU:
                    menu_controller.LoadMenu(MenuType.MAIN);
                    sound_controller.pauseAll();
                    break;
                case GameState.PAUSED:
                    break;
                case GameState.VICTORY:
                    break;
                case GameState.LOSE:
                    break;
            }
            game_state = state;
        }
        #endregion

        #region Getters
        public SoundController getSoundController()
        {
            return sound_controller;
        }
        #endregion

        #region Drawing
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            canvas.Reset();
            canvas.BeginSpritePass(BlendState.AlphaBlend, camera);

            switch (game_state)
            {
                case GameState.IN_GAME:
                    DrawGame(canvas);
                    break;
                case GameState.MAIN_MENU:
                    menu_controller.DrawMenu(canvas, camera.Position);
                    break;
                case GameState.PAUSED:
                    DrawGame(canvas);
                    menu_controller.DrawMenu(canvas, camera.Position);
                    break;
                case GameState.VICTORY:
                    DrawGame(canvas);
                    menu_controller.DrawMenu(canvas, camera.Position);
                    break;
                case GameState.LOSE:
                    DrawGame(canvas);
                    menu_controller.DrawMenu(canvas, camera.Position);
                    break;
            }

            // Draw particles
            particle_engine.Draw(canvas);

            // Draw fade effect
            canvas.DrawSprite(solid, Color.Lerp(new Color(0, 0, 0, 0), new Color(0, 0, 0, 250), (float)fader.fadeCounter / Fader.fadeTime),
                new Rectangle((int)(camera.Position.X - canvas.Width / 2), (int)(camera.Position.Y - canvas.Height / 2), canvas.Width, canvas.Height),
                new Rectangle(0, 0, solid.Width, solid.Height));

            canvas.EndSpritePass();
            base.Draw(gameTime);
        }

        /*
         * Draw in-game graphics
         */
        private void DrawGame(GameCanvas canvas)
        {
            level_controller.Draw(canvas);
            HUD_display.DrawLayerOne(canvas, unit_controller.Units, unit_controller.Player);
            item_controller.Draw(canvas, false);
            unit_controller.Draw(canvas);
            item_controller.Draw(canvas, true);
            HUD_display.DrawLayerTwo(canvas, unit_controller.Units, unit_controller.Player, camera.Position, level_controller.CurLevel);
        }
        #endregion
    }
}
