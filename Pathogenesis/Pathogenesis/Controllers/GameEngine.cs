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

        // Fade timer
        private int fadeCounter = 0;
        private int fadeTime = 1000;

        private SpriteBatch spriteBatch;

        // Used to draw the game onto the screen (VIEW CLASS)
        private GameCanvas canvas;

        // Used to load content and create game objects
        private ContentFactory factory;

        // Game camera, position determines portion of map drawn on screen
        private Camera camera;

        // Game operation controllers
        private InputController input_controller;
        private SoundController sound_controller;
        private CollisionController collision_controller;

        // Game entity controllers
        private GameUnitController unit_controller;
        private ItemController item_controller;
        private LevelController level_controller;
        private MenuController menu_controller;

        private HUD HUD_display;
        private GameState game_state;
        #endregion

        #region Initialization
        public GameEngine()
        {
            canvas = new GameCanvas(this);
            factory = new ContentFactory(new ContentManager(Services));
            camera = new Camera(canvas.Width, canvas.Height);
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

            // Initialize controllers
            input_controller = new InputController();
            sound_controller = new SoundController(factory);
            collision_controller = new CollisionController();

            level_controller = new LevelController();
            unit_controller = new GameUnitController(factory);
            item_controller = new ItemController();
            menu_controller = new MenuController(factory);

            // Game starts at the main menu
            game_state = GameState.MAIN_MENU;
            menu_controller.LoadMenu(MenuType.MAIN);

            // TEST
            //win_menu = factory.createMenu(MenuType.WIN);
            //lose_menu = factory.createMenu(MenuType.LOSE);

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
            
            input_controller.Update();    // Receive and process input
            sound_controller.Update();

            switch (game_state)
            {
                case GameState.IN_GAME:
                    // Remove later
                    Random rand = new Random();
                    if (rand.NextDouble() < 0.02 && unit_controller.Units.Count < 80)
                    {
                        Vector2 pos = new Vector2(rand.Next(level_controller.CurLevel.Width), rand.Next(level_controller.CurLevel.Height));
                        if (level_controller.CurLevel.Map.canMoveToWorldPos(pos))
                        {
                            int level = rand.NextDouble() < 0.0 ? (rand.NextDouble() < 0.2 ? 2 : 2) : 1;
                            unit_controller.AddUnit(factory.createUnit(rand.NextDouble() < 0.1 ? UnitType.FLYING : UnitType.TANK, UnitFaction.ENEMY, level,
                                pos,
                                rand.NextDouble() < 0.3 ? true : false));

                            if (rand.NextDouble() < 0.2)
                            {
                                item_controller.AddItem(factory.createPickup(pos,
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
                        item_controller.AddItem(factory.createPickup(new Vector2(rand.Next(level_controller.CurLevel.Width), rand.Next(level_controller.CurLevel.Height)),
                            ItemType.PLASMID));
                    }
                    //Restart
                    if (input_controller.Restart)
                    {
                        level_controller.Restart(factory, unit_controller, item_controller, sound_controller);
                    }

                    // Process level environment logic
                    bool victory = level_controller.Update();

                    //Auto win
                    if (input_controller.Enter)
                    {
                        victory = true;
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

                    if (unit_controller.Player != null)
                    {
                        camera.Position = unit_controller.Player.Position;
                    }
                    break;
                case GameState.MAIN_MENU:
                    // for now
                    menu_controller.Update(input_controller);
                    Menu menu = menu_controller.CurMenu;
                    if (input_controller.Enter)
                    {
                        switch (menu.Options[menu.CurSelection])
                        {
                            case "Play":
                                game_state = GameState.IN_GAME;
                                level_controller.LoadLevel(factory, unit_controller, item_controller, sound_controller, 0);
                                break;
                            case "Options":
                                break;
                            case "Quit":
                                this.Exit();
                                break;
                        }
                    }
                    break;
                case GameState.PAUSED:
                    menu_controller.Update(input_controller);
                    menu = menu_controller.CurMenu;
                    if (input_controller.Enter)
                    {
                        switch (menu.Options[menu.CurSelection])
                        {
                            case "Resume":
                                game_state = GameState.IN_GAME;
                                break;
                            case "Map":
                                break;
                            case "Options":
                                break;
                            case "Quit to Menu":
                                game_state = GameState.MAIN_MENU;
                                menu_controller.LoadMenu(MenuType.MAIN);
                                sound_controller.pause("music1");
                                break;
                        }
                    }
                    break;
                case GameState.VICTORY:
                    menu_controller.Update(input_controller);
                    menu = menu_controller.CurMenu;
                    if (input_controller.Enter)
                    {
                        switch (menu.Options[menu.CurSelection])
                        {
                            case "Continue":
                                game_state = GameState.IN_GAME;
                                level_controller.NextLevel(factory, unit_controller, item_controller, sound_controller);
                                break;
                        }
                    }
                    break;
                case GameState.LOSE:
                    menu_controller.Update(input_controller);
                    menu = menu_controller.CurMenu;
                    if (input_controller.Enter)
                    {
                        switch (menu.Options[menu.CurSelection])
                        {
                            case "Restart":
                                game_state = GameState.IN_GAME;
                                level_controller.Restart(factory, unit_controller, item_controller, sound_controller);
                                break;
                            case "Quit to Menu":
                                game_state = GameState.MAIN_MENU;
                                menu_controller.LoadMenu(MenuType.MAIN);
                                sound_controller.pause("music1");
                                break;
                        }
                    }
                    break;
            }

            base.Update(gameTime);
        }

        private void fadeTo(GameState state)
        {
            fadeCounter = fadeTime;
            game_state = state;
        }

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
                    //win_menu.Draw(canvas, camera.Position);
                    break;
                case GameState.LOSE:
                    DrawGame(canvas);
                    menu_controller.DrawMenu(canvas, camera.Position);
                    //lose_menu.Draw(canvas, camera.Position);
                    break;
            }

            canvas.EndSpritePass();
            base.Draw(gameTime);
        }

        private void DrawGame(GameCanvas canvas)
        {
            level_controller.Draw(canvas);
            HUD_display.DrawLayerOne(canvas, unit_controller.Units, unit_controller.Player);
            item_controller.Draw(canvas);
            unit_controller.Draw(canvas);
            HUD_display.DrawLayerTwo(canvas, unit_controller.Units, unit_controller.Player, level_controller.CurLevel);
        }
        #endregion
    }
}
