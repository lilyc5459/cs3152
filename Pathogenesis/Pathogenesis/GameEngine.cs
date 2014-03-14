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

namespace Pathogenesis
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class GameEngine : Microsoft.Xna.Framework.Game
    {
        #region Fields
        SpriteBatch spriteBatch;

        // Used to draw the game onto the screen (VIEW CLASS)
        protected GameCanvas canvas;

        // Used to load content and create game objects
        protected ContentFactory factory;

        // Game camera, position determines portion of map drawn on screen
        protected Camera camera;

        // Game operation controllers
        protected InputController input_controller;
        protected CollisionController collision_controller;

        // Game entity controllers
        protected GameUnitController unit_controller;
        protected ItemController item_controller;
        protected LevelController level_controller;

        #endregion

        #region Initialization
        public GameEngine()
        {
            canvas = new GameCanvas(this);
            factory = new ContentFactory(new ContentManager(Services));
            camera = new Camera();

            input_controller = new InputController();
            collision_controller = new CollisionController();

            unit_controller = new GameUnitController();
            item_controller = new ItemController();
            level_controller = new LevelController();
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


            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            canvas.Reset();
            canvas.BeginSpritePass(BlendState.AlphaBlend);
            GameUnit e = factory.createUnit(UnitType.TANK, UnitFaction.ENEMY, new Vector2(0, 0));
            e.draw(canvas);
            Player p = factory.createPlayer(new Vector2(200, 200));
            p.draw(canvas);
            GameUnit a = factory.createUnit(UnitType.TANK, UnitFaction.ALLY, new Vector2(100, 100));
            a.draw(canvas);
            canvas.EndSpritePass();
            base.Draw(gameTime);
        }
        #endregion
    }
}
