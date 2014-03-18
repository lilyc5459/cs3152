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
        PAUSED      // Player has activated the pause menu
    }
    #endregion

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class GameEngine : Microsoft.Xna.Framework.Game
    {
        #region Fields
        private SpriteBatch spriteBatch;

        // Used to draw the game onto the screen (VIEW CLASS)
        private GameCanvas canvas;

        // Used to load content and create game objects
        private ContentFactory factory;

        // Game camera, position determines portion of map drawn on screen
        private Camera camera;

        // Game operation controllers
        private InputController input_controller;
        private CollisionController collision_controller;

        // Game entity controllers
        private GameUnitController unit_controller;
        private ItemController item_controller;
        private LevelController level_controller;

        private HUD HUD_display;
        private GameState game_state;
        #endregion

        #region Initialization
        public GameEngine()
        {
            canvas = new GameCanvas(this);
            factory = new ContentFactory(new ContentManager(Services));
            camera = new Camera(canvas.Width, canvas.Height);

            input_controller = new InputController();
            collision_controller = new CollisionController();

            level_controller = new LevelController();
            unit_controller = new GameUnitController(factory);
            item_controller = new ItemController();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            factory.LoadAllContent();
            canvas.Initialize(this);

            // Game starts at the main menu
            game_state = GameState.MAIN_MENU;

            // TEST
            unit_controller.Player = factory.createPlayer(new Vector2(100, 100));
            HUD_display = factory.createHUD(unit_controller.Player);
            level_controller.NextLevel(factory);
            base.Initialize();
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
            
            input_controller.Update();    // Receive and process input
            switch (game_state)
            {
                case GameState.MAIN_MENU:
                    // for now
                    game_state = GameState.IN_GAME;
                    break;
                case GameState.IN_GAME:
                    // Remove later
                    Random rand = new Random();
                    if (rand.NextDouble() < 0.02 && unit_controller.Units.Count < 20)
                    {
                        unit_controller.AddUnit(factory.createUnit(UnitType.TANK, UnitFaction.ENEMY,
                            new Vector2(rand.Next(level_controller.CurLevel.Width), rand.Next(level_controller.CurLevel.Height))));
                        item_controller.AddItem(factory.createPickup());
                    }
                    //

                    level_controller.Update();          // Process level environment logic
                    unit_controller.Update(             // Process and update all units
                        level_controller.CurLevel, input_controller);
                    HUD_display.Update(                 // Update the HUD
                        input_controller);
                    collision_controller.Update(        // Process and handle collisions
                        unit_controller.Units, unit_controller.Player, level_controller.CurLevel);
                    
                    if (unit_controller.Player != null)
                    {
                        camera.Position = unit_controller.Player.Position;
                    }
                    break;
                case GameState.PAUSED:
                    break;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            canvas.Reset();
            canvas.BeginSpritePass(BlendState.AlphaBlend, camera);

            level_controller.Draw(canvas);
            HUD_display.Draw(canvas, unit_controller.Units);
            item_controller.Draw(canvas);
            unit_controller.Draw(canvas);

            canvas.EndSpritePass();
            base.Draw(gameTime);
        }
        #endregion
    }
}
