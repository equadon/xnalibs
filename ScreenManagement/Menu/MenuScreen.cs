using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Valekhz.ScreenManagement.Menu
{
    public abstract class MenuScreen : GameScreen
    {
        #region Fields & Properties

        private List<MenuEntry> _menuEntries = new List<MenuEntry>();

        protected IList<MenuEntry> MenuEntries
        {
            get { return _menuEntries; }
        }

        public int SelectedEntry { get; private set; }

        public string MenuTitle { get; private set; }

        // Menu actions
        public InputAction MenuUp { get; private set; }
        public InputAction MenuDown { get; private set; }
        public InputAction MenuSelect { get; private set; }
        public InputAction MenuCancel { get; private set; }

        #endregion

        protected MenuScreen(string menuTitle)
        {
            MenuTitle = menuTitle;

            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            MenuUp = new InputAction(null, new Keys[] { Keys.Up }, true);
            MenuDown = new InputAction(null, new Keys[] { Keys.Down }, true);
            MenuSelect = new InputAction(null, new Keys[] { Keys.Enter, Keys.Space }, true);
            MenuCancel = new InputAction(null, new Keys[] { Keys.Escape }, true);
        }

        /// <summary>
        /// Responds to user input, changing the selected entry and accepting
        /// or cancelling the menu.
        /// </summary>
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            // For input tests we pass in our ControllingPlayer, which may
            // either be null (to accept input from any player) or a specific index.
            // If we pass a null controlling player, the InputState helper returns to
            // us which player actually provided the input. We pass that through to
            // OnSelectEntry and OnCancel, so they can tell which player triggered them.
            PlayerIndex playerIndex;

            // Move to the previous menu entry?
            if (MenuUp.Evaluate(input, ControllingPlayer, out playerIndex))
            {
                SelectedEntry--;

                if (SelectedEntry < 0)
                    SelectedEntry = MenuEntries.Count - 1;
            }

            // Move to the next menu entry?
            if (MenuDown.Evaluate(input, ControllingPlayer, out playerIndex))
            {
                SelectedEntry++;

                if (SelectedEntry >= MenuEntries.Count)
                    SelectedEntry = 0;
            }

            if (MenuSelect.Evaluate(input, ControllingPlayer, out playerIndex))
            {
                OnSelectEntry(SelectedEntry, playerIndex);
            }
            else if (MenuCancel.Evaluate(input, ControllingPlayer, out playerIndex))
            {
                OnCancel(playerIndex);
            }
        }

        /// <summary>
        /// Handler for when the user has chosen a menu entry.
        /// </summary>
        protected virtual void OnSelectEntry(int entryIndex, PlayerIndex playerIndex)
        {
            MenuEntries[entryIndex].OnSelectEntry(playerIndex);
        }


        /// <summary>
        /// Handler for when the user has cancelled the menu.
        /// </summary>
        protected virtual void OnCancel(PlayerIndex playerIndex)
        {
            ExitScreen();
        }

        /// <summary>
        /// Helper overload makes it easy to use OnCancel as a MenuEntry event handler.
        /// </summary>
        protected void OnCancel(object sender, PlayerIndexEventArgs e)
        {
            OnCancel(e.PlayerIndex);
        }

        /// <summary>
        /// Allows the screen the chance to position the menu entries. By default
        /// all menu entries are lined up in a vertical list, centered on the screen.
        /// </summary>
        protected virtual void UpdateMenuEntryLocations()
        {
            // Make the menu slide into place during transitions, using a
            // power curve to make things look more interesting (this makes
            // the movement slow down as it nears the end).
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            // start at Y = 175; each X value is generated per entry
            Vector2 position = new Vector2(0f, 175f);

            // update each menu entry's location in turn
            for (int i = 0; i < MenuEntries.Count; i++)
            {
                MenuEntry menuEntry = MenuEntries[i];

                // each entry is to be centered horizontally
                position.X = ScreenManager.GraphicsDevice.Viewport.Width / 2 - menuEntry.GetWidth(this) / 2;

                if (ScreenState == ScreenState.TransitionOn)
                    position.X -= transitionOffset * 256;
                else
                    position.X += transitionOffset * 512;

                // set the entry's position
                menuEntry.Position = position;

                // move down for the next entry the size of this entry
                position.Y += menuEntry.GetHeight(this);
            }
        }

        /// <summary>
        /// Updates the menu.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Update each nested MenuEntry object.
            for (int i = 0; i < MenuEntries.Count; i++)
            {
                bool isSelected = IsActive && (i == SelectedEntry);

                MenuEntries[i].Update(this, isSelected, gameTime);
            }
        }


        /// <summary>
        /// Draws the menu.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // make sure our entries are in the right place before we draw them
            UpdateMenuEntryLocations();

            GraphicsDevice graphics = ScreenManager.GraphicsDevice;
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            spriteBatch.Begin();

            // Draw each menu entry in turn.
            for (int i = 0; i < MenuEntries.Count; i++)
            {
                MenuEntry menuEntry = MenuEntries[i];

                bool isSelected = IsActive && (i == SelectedEntry);

                menuEntry.Draw(this, isSelected, gameTime);
            }

            // Make the menu slide into place during transitions, using a
            // power curve to make things look more interesting (this makes
            // the movement slow down as it nears the end).
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            // Draw the menu title centered on the screen
            Vector2 titlePosition = new Vector2(graphics.Viewport.Width / 2f, 80);
            Vector2 titleOrigin = font.MeasureString(MenuTitle) / 2;
            Color titleColor = new Color(50, 50, 50) * TransitionAlpha;
            float titleScale = 1.25f;

            titlePosition.Y -= transitionOffset * 100;

            spriteBatch.DrawString(font, MenuTitle, titlePosition, titleColor, 0,
                                   titleOrigin, titleScale, SpriteEffects.None, 0);

            spriteBatch.End();
        }
    }
}
