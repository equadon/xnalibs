using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Valekhz.ScreenManagement.Menu
{
    public class MenuEntry
    {
        #region Fields & Properties

        private float _selectionFade;
        private Vector2 _position;

        public string Text { get; protected set; }

        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        #endregion

        #region Events

        /// <summary>Event raised when the menu entry is selected.</summary>
        public event EventHandler<PlayerIndexEventArgs> Selected;

        /// <summary>Method for raising the Selected event.</summary>
        /// <param name="playerIndex"></param>
        protected internal virtual void OnSelectEntry(PlayerIndex playerIndex)
        {
            if (Selected != null)
                Selected(this, new PlayerIndexEventArgs(playerIndex));
        }

        #endregion

        public MenuEntry(string text)
        {
            Text = text;
        }

        /// <summary>Update menu entry.</summary>
        public virtual void Update(MenuScreen screen, bool isSelected, GameTime gameTime)
        {
            // Gradually fade between selected/deselected appearance
            float fadeSpeed = (float) gameTime.ElapsedGameTime.TotalSeconds * 4;

            if (isSelected)
                _selectionFade = Math.Min(_selectionFade + fadeSpeed, 1);
            else
                _selectionFade = Math.Max(_selectionFade - fadeSpeed, 0);
        }

        /// <summary>
        /// Draws the menu entry. This can be overridden to customize the appearance.
        /// </summary>
        public virtual void Draw(MenuScreen screen, bool isSelected, GameTime gameTime)
        {
            Color color = isSelected ? Color.Yellow : Color.Black;

            // Pulsate thet size of the selected entry
            double time = gameTime.TotalGameTime.TotalSeconds;

            float pulsate = (float) Math.Sin(time * 6) + 1;

            float scale = 1 + pulsate * 0.05f * _selectionFade;

            // Modify the alpha to fade text out during transitions.
            color *= screen.TransitionAlpha;

            // Draw text, centered on the middle of each line
            ScreenManager screenManager = screen.ScreenManager;
            SpriteBatch spriteBatch = screenManager.SpriteBatch;
            SpriteFont font = screenManager.Font;

            Vector2 origin = new Vector2(0, font.LineSpacing / 2f);

            spriteBatch.DrawString(font, Text, Position, color, 0f,
                origin, scale, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Queries how much space this menu entry requires.
        /// </summary>
        public virtual int GetHeight(MenuScreen screen)
        {
            return screen.ScreenManager.Font.LineSpacing;
        }


        /// <summary>
        /// Queries how wide the entry is, used for centering on the screen.
        /// </summary>
        public virtual int GetWidth(MenuScreen screen)
        {
            return (int)screen.ScreenManager.Font.MeasureString(Text).X;
        }
    }
}
