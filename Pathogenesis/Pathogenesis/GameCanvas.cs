#region File Description
//-----------------------------------------------------------------------------
// GameCanvas.cs
//
// Primary view class for the game.
//
// After three labs, you should now be familiar with this class.  This class
// encapsulates the various drawing features of XNA/MonoGame and puts them
// behind a single interface.
//
// This is the most general version of GameCanvas among the four labs.  It
// supports both Sprite graphics and Polygons.  Using Polygons requires a
// a bit of understanding of 3D graphics, but we will talk about this in
// class.
//
// Like previous labs, this class draws in multiple passes. However, there
// is no requirement on the order of each pass. In fact, each pass can be
// called multiple times: you can draw a sprite pass, followed by a polygon
// pass, followed by a sprite pass, all in the same animation frame.  
//
// The purpose of these passes is to keep you from drawing Sprites and
// polygons at the same time.  The graphics is not designed to handle this,
// and we must keep these separate as distinct passes.
//
// For 99% of you in this class, this GameCanvas is all you will ever 
// need for your game. If you use it in your final product, make sure
// to cite its source.
//
// Author: Walker M. White
// Based on original PhysicsDemo Lab by Don Holden, 2007
// MonoGame version, 2/14/2014
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

/// <summary>
/// The namespace, or package, of this application.
/// </summary>
/// <remarks>
/// All C# programs have a namespace.  They function the same way that Java
/// packages do  Like Java, your directory must store the source code in 
/// folders with the same name as the namespace.
/// </remarks>
namespace Pathogenesis
{

    #region Enum
    /// <summary>
    /// Draw states to keep track of our current drawing pass
    /// </summary>
    /// <remarks>
    /// There is no order on drawing passes for this canvas, but the canvas
    /// can only be in one drawing pass at a time.  If you wish to change
    /// the current drawing state, you must ifrst end any pass that is 
    /// currently active.  In particular, the sprite pass is not drawn to
    /// the screen until you end it.
    /// </remarks>
    public enum DrawState
    {
        Inactive,       // Not in the middle of any pass.
        SpritePass,     // In the middle of a SpriteBatch pass.
        PolygonPass     // In the middle of a Polygon pass.
    };
    #endregion

    /// <summary>
    /// Abstract the graphics features for ease of use.
    /// </summary>
    /// <remarks>
    /// This class supports both sprite and polygonal drawing.  Normally, these
    /// are provided by different classes; this interface gathers them 
    /// together in a single interface.
    /// 
    /// The canvas has two passes, but there is no restriction on the order
    /// called.  You can also repeat a pass in a single animation frame.
    /// The purpose of the passes is to force you end a Sprite pass before
    /// drawing polygons and vice versa.
    /// </remarks>
    public class GameCanvas
    {

        #region Constants
        // Default window size.  This belongs in the view, not the game engine.
        private const int GAME_WIDTH = 800;
        private const int GAME_HEIGHT = 600;
        #endregion

        #region Fields
        // Used to track window properties
        protected GraphicsDeviceManager graphics;
        protected GameWindow window;
        protected Rectangle bounds;

        // For drawing sprites
        protected SpriteBatch spriteBatch;

        // For drawing polygons
        protected BasicEffect effect;

        // Track the current drawing pass. 
        protected DrawState state;

        // For onscreen messages
        protected SpriteFont font;

        // Private variable for property IsFullscreen.
        protected bool fullscreen;

        // Attributes to rescale the image
        protected Matrix transform;
        protected Vector2 worldScale;
        #endregion

        #region Properties (READ-WRITE)
        /// <summary>
        /// Whether this instance is fullscreen.
        /// </summary>
        /// <remarks>
        /// This value cannot be reset during an active drawing pass.
        /// </remarks>
        public bool IsFullscreen
        {
            get { return fullscreen; }
            set
            {
                Debug.Assert(state == DrawState.Inactive, "Cannot reset property while actively drawing");
                if (fullscreen != value && graphics != null)
                {
                    graphics.IsFullScreen = value;
                    graphics.ApplyChanges();
                }
                fullscreen = value;
            }
        }

        /// <summary>
        /// The rectangular bounds of the drawing Window.
        /// </summary>
        /// <remarks>
        /// This code has some VERY Windows specific code in it that is
        /// incompatible with MonoGame. If you wish to use MonoGame, you 
        /// need to use the GameCanvas version for that engine.
        /// 
        /// This value cannot be reset during an active drawing pass.
        /// </remarks>
        public Rectangle Bounds
        {
            get
            {
                // Update bounds to the current state
                if (graphics != null)
                {
                    bounds.X = window.ClientBounds.X;
                    bounds.Y = window.ClientBounds.Y;
                    bounds.Width = graphics.PreferredBackBufferWidth;
                    bounds.Height = graphics.PreferredBackBufferHeight;
                }
                return bounds;
            }
            set
            {
                Debug.Assert(state == DrawState.Inactive, "Cannot reset property while actively drawing");
                bounds = value;
                if (graphics != null)
                {
                    // Code to change window size
                    graphics.PreferredBackBufferWidth = bounds.Width;
                    graphics.PreferredBackBufferHeight = bounds.Height;
                    graphics.ApplyChanges();

                    // Code to change window position
                    //System.Windows.Forms.Form form = (System.Windows.Forms.Form)System.Windows.Forms.Control.FromHandle(window.Handle);
                    //form.Location = new System.Drawing.Point(bounds.X, bounds.Y);
                }
            }
        }

        /// <summary>
        /// The left side of this game window
        /// </summary>
        /// <remarks>
        /// This code has some VERY Windows specific code in it that is
        /// incompatible with MonoGame. If you wish to use MonoGame, you 
        /// need to use the GameCanvas version for that engine.
        /// 
        /// This value cannot be reset during an active drawing pass.
        /// </remarks>
        public int X
        {
            get
            {
                if (window != null && state == DrawState.Inactive)
                {
                    bounds.X = window.ClientBounds.X;
                }
                return bounds.X;
            }
            set
            {
                Debug.Assert(state == DrawState.Inactive, "Cannot reset property while actively drawing");
                bounds.X = value;
                if (window != null)
                {
                    //System.Windows.Forms.Form form = (System.Windows.Forms.Form)System.Windows.Forms.Control.FromHandle(window.Handle);
                    //form.Location = new System.Drawing.Point(bounds.X, bounds.Y);
                }
            }
        }

        /// <summary>
        /// The upper side of this game window.
        /// </summary>
        /// <remarks>
        /// This code has some VERY Windows specific code in it that is
        /// incompatible with MonoGame. If you wish to use MonoGame, you 
        /// need to use the GameCanvas version for that engine.
        /// 
        /// This value cannot be reset during an active drawing pass.
        /// </remarks>
        public int Y
        {
            get
            {
                if (window != null && state == DrawState.Inactive)
                {
                    bounds.Y = window.ClientBounds.Y;
                }
                return bounds.Y;
            }
            set
            {
                Debug.Assert(state == DrawState.Inactive, "Cannot reset property while actively drawing");
                bounds.Y = value;
                if (window != null)
                {
                    //System.Windows.Forms.Form form = (System.Windows.Forms.Form)System.Windows.Forms.Control.FromHandle(window.Handle);
                    //form.Location = new System.Drawing.Point(bounds.X, bounds.Y);
                }
            }
        }

        /// <summary>
        /// The width of this drawing canvas
        /// </summary>
        /// <remarks>
        /// This value cannot be reset during an active drawing pass.
        /// </remarks>
        public int Width
        {
            get
            {
                if (graphics != null && state == DrawState.Inactive)
                {
                    bounds.Width = graphics.PreferredBackBufferWidth;
                }
                return bounds.Width;
            }
            set
            {
                Debug.Assert(state == DrawState.Inactive, "Cannot reset property while actively drawing");
                bounds.Width = value;
                if (graphics != null && state == DrawState.Inactive)
                {
                    graphics.PreferredBackBufferWidth = value;
                    graphics.ApplyChanges();
                }
            }
        }

        /// <summary>
        /// The height of this drawing canvas
        /// </summary>
        /// <remarks>
        /// This value cannot be reset during an active drawing pass.
        /// </remarks>
        public int Height
        {
            get
            {
                if (graphics != null)
                {
                    bounds.Height = graphics.PreferredBackBufferHeight;
                }
                return bounds.Height;
            }
            set
            {
                Debug.Assert(state == DrawState.Inactive, "Cannot reset property while actively drawing");
                bounds.Height = value;
                if (graphics != null)
                {
                    graphics.PreferredBackBufferWidth = value;
                    graphics.ApplyChanges();
                }
            }
        }

        /// <summary>
        /// The global scale of this drawing canvas
        /// </summary>
        /// <remarks>
        /// Altering this value will zoom in or out of the canvas (anchored at 
        /// the bottom left corner).
        /// 
        /// This value cannot be reset during an active drawing pass.
        /// </remarks>
        public Vector2 Scale
        {
            get { return worldScale; }
            set
            {
                Debug.Assert(state == DrawState.Inactive, "Cannot reset property while actively drawing");
                Debug.Assert(value.X > 0 && value.Y > 0, "Scale attributes must be positive");
                worldScale = value;
                transform = Matrix.CreateScale(worldScale.X, worldScale.Y, 1.0f);
            }
        }

        /// <summary>
        /// The global X-axis scale of this drawing canvas
        /// </summary>
        /// <remarks>
        /// This value cannot be reset during an active drawing pass.
        /// </remarks>
        public float SX
        {
            get { return worldScale.X; }
            set
            {
                Debug.Assert(state == DrawState.Inactive, "Cannot reset property while actively drawing");
                Debug.Assert(value > 0, "Scale attributes must be positive");
                worldScale.X = value;
                transform = Matrix.CreateScale(worldScale.X, worldScale.Y, 1.0f);
            }
        }

        /// <summary>
        /// The global Y-axis scale of this drawing canvas
        /// </summary>
        /// <remarks>
        /// This value cannot be reset during an active drawing pass.
        /// </remarks>
        public float SY
        {
            get { return worldScale.Y; }
            set
            {
                Debug.Assert(state == DrawState.Inactive, "Cannot reset property while actively drawing");
                Debug.Assert(value > 0, "Scale attributes must be positive");
                worldScale.Y = value;
                transform = Matrix.CreateScale(worldScale.X, worldScale.Y, 1.0f);
            }
        }

        /// <summary>
        /// The font for displaying messages.
        /// </summary>
        public SpriteFont Font
        {
            get { return font; }
            set { font = value; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor to create a new instance of our canvas.
        /// </summary>
        /// <remarks>
        /// Note that we initialize the graphics device manager as soon
        /// as this canvas is constructed. However, we do NOT create a
        /// new SpriteBatch yet.  We have to wait for the graphics manager 
        /// to initialize before we do that. 
        /// </remarks>
        /// <param name="game">The root game engine for this canvas</param>
        public GameCanvas(Game game)
        {
            // Create a new graphics manager.
            fullscreen = false;
            graphics = new GraphicsDeviceManager(game);
            graphics.IsFullScreen = fullscreen;

            bounds.Width = GAME_WIDTH;
            bounds.Height = GAME_HEIGHT;

            Scale = Vector2.One;
        }

        /// <summary>
        /// Initialize the SpriteBatch for this canvas and prepare for drawing.
        /// </summary>
        /// <remarks>
        /// This method is called by the Initialize method of the game engine,
        /// after all of the game has finished all necessary off-screen
        /// initialization.
        /// </remarks>
        /// <param name="game">The root game engine for this canvas</param>
        public void Initialize(Game game)
        {
            // Override window position at start-up
            window = game.Window;
            if (!fullscreen)
            {
                graphics.PreferredBackBufferWidth = bounds.Width;
                graphics.PreferredBackBufferHeight = bounds.Height;
                graphics.ApplyChanges();
            }

            // Set up Sprite Batch properites
            spriteBatch = new SpriteBatch(game.GraphicsDevice);

            // Set up polygon properties
            effect = new BasicEffect(game.GraphicsDevice);
            effect.View = Matrix.Identity;
            effect.Projection = Matrix.CreateOrthographicOffCenter(0, bounds.Width, bounds.Height, 0, 0, 1);
            effect.TextureEnabled = true;
            effect.LightingEnabled = false;

            // We are not actively drawing
            state = DrawState.Inactive;
        }

        /// <summary>
        /// Load all default graphics resources for the canvas
        /// </summary>
        /// <param name='content'>
        /// Reference to global content manager.
        /// </param>
        public void LoadContent(ContentFactory factory)
        {
            // Load sprite font
            font = factory.getFont();
        }

        /// <summary>
        /// Eliminate any resources that prevent garbage collection.
        /// </summary>
        public void Dispose()
        {
            graphics = null;
            window = null;
            effect.Dispose();
        }
        #endregion

        #region Drawing Methods
        /// <summary>
        /// Clear the canvas and reset the drawing state for a new animation frame.
        /// </summary>
        public void Reset()
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            // Allow either pass to follow.
            state = DrawState.Inactive;
        }

        #region Sprite Pass

        /// <summary>
        /// Start a new pass to draw sprites
        /// </summary>
        /// <remarks>
        /// Once a pass has begin, you cannot change attributes or draw polygons
        /// until it has ended.
        /// </remarks>
        /// <param name="blend">Blending mode for combining sprites</param>
        public void BeginSpritePass(BlendState blend)
        {
            // Check that state invariant is satisfied.
            Debug.Assert(state == DrawState.Inactive, "Drawing state is invalid (expected Inactive)");
            state = DrawState.SpritePass;

            // Set up the drawing canvas to use the appropriate blending.
            // Deferred sorting guarantees Sprites are drawn in order given.
            spriteBatch.Begin(SpriteSortMode.Deferred, blend, null, null, null, null, transform);
        }

        /// <summary>
        /// Draw a sprite on this drawing canvas.
        /// </summary>
        /// <remarks>
        /// The image is scaled according to the canvas Scale attribute.
        /// </remarks>
        /// <param name="image">Sprite to draw</param>
        /// <param name="tint">Color to tint sprite</param>
        /// <param name="position">Location to draw image on canvas</param>
        public void DrawSprite(Texture2D image, Color tint, Vector2 position)
        {
            // Enforce pass invariants.
            Debug.Assert(state == DrawState.SpritePass, "Drawing state is invalid (expected SpritePass)");

            // Re-center the object.
            position.X -= image.Width / 2;
            position.Y -= image.Height / 2;

            // Draw it.
            spriteBatch.Draw(image, position, tint);
        }

        /// <summary>
        /// Draw a sprite on this drawing canvas.
        /// </summary>
        /// <remarks>
        /// The image is scaled according to the canvas Scale attribute.
        /// </remarks>
        /// <param name="image">Sprite to draw</param>
        /// <param name="tint">Color to tint sprite</param>
        /// <param name="position">Location to draw image on canvas</param>
        /// <param name="scale">Amount to scale image (in addition to global scale)</param>
        /// <param name="angle">Amount to rotate image in radians</param>
        public void DrawSprite(Texture2D image, Color tint, Vector2 position, Vector2 scale, float angle)
        {
            // Enforce invariants.
            Debug.Assert(state == DrawState.SpritePass, "Drawing state is invalid (expected SpritePass)");

            // Get the texture center.
            Vector2 origin = new Vector2(image.Width / 2, image.Height / 2);

            // Draw it.
            spriteBatch.Draw(image, position, null, tint, angle, origin, scale, SpriteEffects.None, 0);
        }

        /// <summary>
        /// Draw a sprite on this drawing canvas.
        /// </summary>
        /// <remarks>
        /// The image is scaled according to the canvas Scale attribute.
        /// </remarks>
        /// <param name="image">Sprite to draw</param>
        /// <param name="tint">Color to tint sprite</param>
        /// <param name="position">Location to draw image on canvas</param>
        /// <param name="scale">Amount to scale image (in addition to global scale)</param>
        /// <param name="angle">Amount to rotate image in radians</param>
        /// <param name="effects">Sprite effect to flip image</param>
        public void DrawSprite(Texture2D image, Color tint, Vector2 position, Vector2 scale, float angle, SpriteEffects effects)
        {
            // Enforce invariants.
            Debug.Assert(state == DrawState.SpritePass, "Drawing state is invalid (expected SpritePass)");

            // Get the texture center.
            Vector2 origin = new Vector2(image.Width / 2, image.Height / 2);

            // Draw it.
            spriteBatch.Draw(image, position, null, tint, angle, origin, scale, effects, 0);
        }

        /// <summary>
        /// Animate a sprite on this drawing canvas.
        /// </summary>
        /// <remarks>
        /// This version of the drawing method will animate an image over a 
        /// filmstrip. It assumes that the filmstrip is a SINGLE LINE of 
        /// images.  You must modify the code if this is not the case.
        /// </remarks>
        /// <param name="image">Sprite to draw</param>
        /// <param name="tint">Color to tint sprite</param>
        /// <param name="position">Location to draw image on canvas</param>
        /// <param name="scale">Amount to scale image (in addition to global scale)</param>
        /// <param name="angle">Amount to rotate image in radians</param>
        /// <param name="frame">Current animation frame</param>
        /// <param name="framesize">Number of frames in filmstrip</param>
        public void DrawSprite(Texture2D image, Color tint, Vector2 position, Vector2 scale, float angle, int frame, int framesize)
        {
            // Enforce invariants.
            Debug.Assert(state == DrawState.SpritePass, "Drawing state is invalid (expected SpritePass)");

            // Pick out the right frame
            int width = image.Width / framesize;
            int height = image.Height;

            // Compute frame position assuming only 1 row of frames.
            Rectangle src = new Rectangle(frame * width, 0, width, height);
            Vector2 origin = new Vector2(width / 2, height / 2);

            // Draw it.
            spriteBatch.Draw(image, position, src, tint, angle, origin, scale, SpriteEffects.None, 0);
        }

        /// <summary>
        /// Draw an unscaled overlay image.
        /// </summary>
        /// <remarks>
        /// An overlay image is one that is not scaled according to the current zoom.
        /// This is ideal for backgrounds, foregrounds and uniform HUDs that do not
        /// track the camera. Images are put in the center of the window.
        /// </remarks>
        /// <param name="image">Sprite to draw</param>
        /// <param name="tint">Color to tint sprite</param>
        /// <param name="fill">Whether to stretch the image to fill the window</param>
        public void DrawOverlay(Texture2D image, Color tint, bool fill)
        {
            // Enforce invariants.
            Debug.Assert(state == DrawState.SpritePass, "Drawing state is invalid (expected SpritePass)");

            Vector2 pos = new Vector2(Width, Height) / (2 * Scale);
            Vector2 orig = new Vector2(image.Width / 2, image.Height / 2);
            Vector2 scale = new Vector2(1 / SX, 1 / SY); // To counter global scale
            if (fill)
            {
                scale.X = Width / (image.Width * SX);
                scale.Y = Height / (image.Height * SY);
            }

            // Draw this unscaled
            spriteBatch.Draw(image, pos, null, tint, 0.0f, orig, scale, SpriteEffects.None, 0);
        }

        /// <summary>
        /// Draw an unscaled overlay image.
        /// </summary>
        /// <remarks>
        /// An overlay image is one that is not scaled according to the current zoom.
        /// This is ideal for backgrounds, foregrounds and uniform HUDs that do not
        /// track the camera.
        /// </remarks>
        /// <param name="image">Sprite to draw</param>
        /// <param name="tint">Color to tint sprite</param>
        /// <param name="pos">Position to draw in WINDOW COORDS</param>
        public void DrawOverlay(Texture2D image, Color tint, Vector2 pos)
        {
            // Enforce invariants.
            Debug.Assert(state == DrawState.SpritePass, "Drawing state is invalid (expected SpritePass)");

            // Rescale position to align
            pos = pos / Scale;
            Vector2 orig = new Vector2(image.Width / 2, image.Height / 2);
            Vector2 scale = new Vector2(1 / SX, 1 / SY); // To counter global scale

            // Draw this unscaled
            spriteBatch.Draw(image, pos, null, tint, 0.0f, orig, scale, SpriteEffects.None, 0);
        }

        /// <summary>
        /// Draw text to the screen.
        /// </summary>
        /// <remarks>
        /// Text is scaled by the global scale factor, just like any other image.
        /// Text is drawn with the built-in canvas font.
        /// </remarks>
        /// <param name="text">Text to draw</param>
        /// <param name="tint">Text color</param>
        /// <param name="position">Location to draw text</param>
        public void DrawText(String text, Color tint, Vector2 position)
        {
            // Enforce invariants.
            Debug.Assert(state == DrawState.SpritePass, "Drawing state is invalid (expected SpritePass)");
            spriteBatch.DrawString(font, text, position * Scale, tint);
        }

        /// <summary>
        /// Draw text to the screen.
        /// </summary>
        /// <remarks>
        /// Text is scaled by the global scale factor, just like any other image.
        /// </remarks>
        /// <param name="text">Text to draw</param>
        /// <param name="tint">Text color</param>
        /// <param name="position">Location to draw text</param>
        /// <param name="font">Alternate font to use</param>
        public void DrawText(String text, Color tint, Vector2 position, SpriteFont font)
        {
            // Enforce invariants.
            Debug.Assert(state == DrawState.SpritePass, "Drawing state is invalid (expected SpritePass)");
            spriteBatch.DrawString(font, text, position * Scale, tint);
        }

        /// <summary>
        /// End the sprite pass, flushing all graphics to the screen.
        /// </summary>
        public void EndSpritePass()
        {
            // Check the drawing state invariants.
            Debug.Assert(state == DrawState.SpritePass, "Drawing state is invalid (expected SpritePass)");
            state = DrawState.Inactive;
            spriteBatch.End();
        }

        #endregion

        #region Polygon Pass

        /// <summary>
        /// Start a new pass to draw polygons
        /// </summary>
        /// <remarks>
        /// Once a pass has begin, you cannot change attributes or draw sprites
        /// until it has ended.
        /// </remarks>
        public void BeginPolygonPass()
        {
            // Check that state invariant is satisfied.
            Debug.Assert(state == DrawState.Inactive, "Drawing state is invalid (expected Inactive)");
            state = DrawState.PolygonPass;
        }

        /// <summary>
        /// Draw the given textured polygon.
        /// </summary>
        /// <remarks>
        /// The polygon is scaled according to the canvas Scale attribute.
        /// </remarks>
        /// <param name="vertices">Vertices with texture mapping</param>
        /// <param name="texture">Texture to apply to polygon</param>
        public void DrawPolygons(VertexPositionTexture[] vertices, Texture2D texture)
        {
            DrawPolygons(vertices, texture, Vector2.Zero, 0.0f, 1.0f, BlendState.AlphaBlend);
        }

        /// <summary>
        /// Draw the given textured polygon.
        /// </summary>
        /// <remarks>
        /// The polygon is scaled according to the canvas Scale attribute.
        /// </remarks>
        /// <param name="vertices">Vertices with texture mapping</param>
        /// <param name="texture">Texture to apply to polygon</param>
        /// <param name="position">Position to offset polygon</param>
        /// <param name="angle">Angle to rotate polygon</param>
        /// <param name="scale">Amount to scale polygon</param>
        public void DrawPolygons(VertexPositionTexture[] vertices, Texture2D texture,
                                 Vector2 position, float angle, float scale)
        {
            DrawPolygons(vertices, texture, position, angle, scale, BlendState.AlphaBlend);
        }

        /// <summary>
        /// Draw the given textured polygon.
        /// </summary>
        /// <remarks>
        /// The polygon is scaled according to the canvas Scale attribute.
        /// </remarks>
        /// <param name="vertices">Vertices with texture mapping</param>
        /// <param name="texture">Texture to apply to polygon</param>
        /// <param name="position">Position to offset polygon</param>
        /// <param name="angle">Angle to rotate polygon</param>
        /// <param name="scale">Amount to scale polygon</param>
        /// <param name="blendMode">Blend mode to combine textures</param>
        public void DrawPolygons(VertexPositionTexture[] vertices, Texture2D texture,
                                 Vector2 position, float angle, float scale,
                                 BlendState blendMode)
        {
            // Check the drawing state invariants.
            Debug.Assert(state == DrawState.PolygonPass, "Drawing state is invalid (expected PolygonPass)");

            // Create translation matrix
            effect.World = Matrix.CreateRotationZ(angle) * Matrix.CreateScale(scale) * Matrix.CreateTranslation(new Vector3(position, 0)) * transform;
            effect.Texture = texture;

            // Prepare device for drawing.
            GraphicsDevice device = graphics.GraphicsDevice;
            device.BlendState = blendMode;

            SamplerState s = new SamplerState();
            s.AddressU = TextureAddressMode.Wrap;
            s.AddressV = TextureAddressMode.Wrap;
            device.SamplerStates[0] = s;

            // Draw the polygon
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, vertices, 0, vertices.Length / 3);
            }
        }

        /// <summary>
        /// End the polygon pass.
        /// </summary>
        public void EndPolygonPass()
        {
            // Check that state invariant is satisfied.
            Debug.Assert(state == DrawState.PolygonPass, "Drawing state is invalid (expected PolygonPass)");
            state = DrawState.Inactive;
        }

        #endregion

        #endregion

    }

}

