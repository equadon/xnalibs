using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Valekhz.Components;

namespace Valekhz.ScreenManagement
{
    /// <summary>
    /// The screen manager is a component which manages one or more GameScreen
    /// instances. It maintains a stack of screens, calls their Update and Draw
    /// methods at the appropriate times, and automatically routes input to the
    /// topmost active screen.
    /// </summary>
    public class ScreenManager : GraphicsDeviceDrawableComponent
    {
        #region Fields

        private const string StateFilename = "ScreenManagerState.xml";

        private readonly List<GameScreen> _screens = new List<GameScreen>();
        private readonly List<GameScreen> _tempScreensList = new List<GameScreen>();

        private readonly InputState _input = new InputState();

        #endregion

        #region Properties

        /// <summary>
        /// A default SpriteBatch shared by all the screens. This saves
        /// each screen having to bother creating their own local instance.
        /// </summary>
        public SpriteBatch SpriteBatch { get; private set; }

        /// <summary>
        /// A default ContentManager shared by all the screens. This saves
        /// each screen having to bother creating their own local instance.
        /// </summary>
        public ContentManager Content { get; private set; }

        /// <summary>
        /// A default font shared by all the screens. This saves
        /// each screen having to bother loading their own local copy.
        /// </summary>
        public SpriteFont Font { get; protected set; }

        /// <summary>
        /// Gets a blank texture that can be used by the screens.
        /// </summary>
        public Texture2D BlankTexture { get; protected set; }

        /// <summary>
        /// If true, the manager prints out a list of all the screens
        /// each time it is updated. This can be useful for making sure
        /// everything is being added and removed at the right times.
        /// </summary>
        public bool TraceEnabled { get; set; }

        public bool IsInitialized { get; private set; }

        #endregion

        /// <summary>
        /// Constructs a new screen manager component.
        /// </summary>
        public ScreenManager(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            IsInitialized = false;

            Content = new ContentManager(serviceProvider, "Content");
        }

        #region Initialization

        /// <summary>
        /// Initializes the screen manager component.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            IsInitialized = true;
        }

        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            foreach (GameScreen screen in _screens)
            {
                screen.Activate(false);
            }
        }

        /// <summary>
        /// Unload your graphics content.
        /// </summary>
        protected override void UnloadContent()
        {
            foreach (GameScreen screen in _screens)
            {
                screen.Unload();
            }
        }

        #endregion

        #region Update & Draw

        /// <summary>
        /// Allows each screen to run logic.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // Read the keyboard and gamepad.
            _input.Update();

            // Make a copy of the master screen list, to avoid confusion if
            // the process of updating one screen adds or removes others.
            _tempScreensList.Clear();

            foreach (GameScreen screen in _screens)
                _tempScreensList.Add(screen);

            bool otherScreenHasFocus = false; // TODO: used to be assigned: = !Game.IsActive;
            bool coveredByOtherScreen = false;

            // Loop as long as there are screens waiting to be updated.
            while (_tempScreensList.Count > 0)
            {
                // Pop the topmost screen off the waiting list.
                GameScreen screen = _tempScreensList[_tempScreensList.Count - 1];

                _tempScreensList.RemoveAt(_tempScreensList.Count - 1);

                // Update the screen.
                screen.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

                if (screen.ScreenState == ScreenState.TransitionOn ||
                    screen.ScreenState == ScreenState.Active)
                {
                    // If this is the first active screen we came across,
                    // give it a chance to handle input.
                    if (!otherScreenHasFocus)
                    {
                        screen.HandleInput(gameTime, _input);

                        otherScreenHasFocus = true;
                    }

                    // If this is an active non-popup, inform any subsequent
                    // screens that they are covered by it.
                    if (!screen.IsPopup)
                        coveredByOtherScreen = true;
                }
            }

            // Print debug trace?
            if (TraceEnabled)
                TraceScreens();
        }

        /// <summary>
        /// Prints a list of all the screens, for debugging.
        /// </summary>
        private void TraceScreens()
        {
            List<string> screenNames = new List<string>();

            foreach (GameScreen screen in _screens)
                screenNames.Add(screen.GetType().Name);

            Debug.WriteLine(string.Join(", ", screenNames.ToArray()));
        }


        /// <summary>
        /// Tells each screen to draw itself.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            foreach (GameScreen screen in _screens)
            {
                if (screen.ScreenState == ScreenState.Hidden)
                    continue;

                screen.Draw(gameTime);
            }
        }

        #endregion

        /// <summary>
        /// Adds a new screen to the screen manager.
        /// </summary>
        public void AddScreen(GameScreen screen, PlayerIndex? controllingPlayer)
        {
            screen.ControllingPlayer = controllingPlayer;
            screen.ScreenManager = this;
            screen.IsExiting = false;

            // If we have a graphics device, tell the screen to load content.
            if (IsInitialized)
            {
                screen.Activate(false);
            }

            _screens.Add(screen);
        }

        /// <summary>
        /// Removes a screen from the screen manager. You should normally
        /// use GameScreen.ExitScreen instead of calling this directly, so
        /// the screen can gradually transition off rather than just being
        /// instantly removed.
        /// </summary>
        public void RemoveScreen(GameScreen screen)
        {
            // If we have a graphics device, tell the screen to unload content.
            if (IsInitialized)
            {
                screen.Unload();
            }

            _screens.Remove(screen);
            _tempScreensList.Remove(screen);
        }


        /// <summary>
        /// Expose an array holding all the screens. We return a copy rather
        /// than the real master list, because screens should only ever be added
        /// or removed using the AddScreen and RemoveScreen methods.
        /// </summary>
        public GameScreen[] GetScreens()
        {
            return _screens.ToArray();
        }


        /// <summary>
        /// Helper draws a translucent black fullscreen sprite, used for fading
        /// screens in and out, and for darkening the background behind popups.
        /// </summary>
        public void FadeBackBufferToBlack(float alpha)
        {
            SpriteBatch.Begin();
            SpriteBatch.Draw(BlankTexture, GraphicsDevice.Viewport.Bounds, Color.Black * alpha);
            SpriteBatch.End();
        }

        /// <summary>
        /// Informs the screen manager to serialize its state to disk.
        /// </summary>
        public void Deactivate()
        {
        }

        public bool Activate(bool instancePreserved)
        {
            return false;
        }
    }
}
