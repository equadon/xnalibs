using System;
using Microsoft.Xna.Framework;

namespace Valekhz.Components
{
    /// <summary>
    ///   Lightweight variant DrawableGameComponent that doesn't reference the Game class
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     This is a lightweight version of DrawableGameComponent that can be used
    ///     without requiring a Game class to be present. Useful to get all the
    ///     advantages of the XNA GameServices architecture even when you have
    ///     initialized and manage the graphics device yourself.
    ///   </para>
    ///   <para>
    ///     The name of this class is the same as 'DrawableGameComponent' minus the
    ///     'Game' part as the Game reference is what this class removes from its namesake.
    ///   </para>
    /// </remarks>
    public class DrawableComponent : Component, IDrawable
    {
        /// <summary>Triggered when the value of the draw order property is changed.</summary>
        public event EventHandler<EventArgs> DrawOrderChanged;

        /// <summary>Triggered when the value of the visibilty property is changed.</summary>
        public event EventHandler<EventArgs> VisibleChanged;

        #region Fields & Properties

        private bool _visible;
        private int _drawOrder;

        /// <summary>
        /// Indicates when the drawable component should be drawn in relation to other
        /// drawables. Has no effect by itself.
        /// </summary>
        public int DrawOrder
        {
            get { return _drawOrder; }
            set
            {
                if (value != _drawOrder)
                {
                    _drawOrder = value;
                    OnDrawOrderChanged();
                }
            }
        }

        /// <summary>Tru when the drawable component is visible and should be drawn.</summary>
        public bool Visible
        {
            get { return _visible; }
            set
            {
                if (value != _visible)
                {
                    _visible = value;
                    OnDrawOrderChanged();
                }
            }
        }

        #endregion

        public DrawableComponent()
        {
            _visible = true;
        }

        #region Implementation of IDrawable

        /// <summary>Called when the drawable component needs to draw itself</summary>
        /// <param name="gameTime">Provides a snapshot of the game's timing values</param>
        public virtual void Draw(GameTime gameTime) { }

        #endregion

        protected virtual void OnVisibleChanged()
        {
            if (VisibleChanged != null)
                VisibleChanged(this, EventArgs.Empty);
        }

        protected virtual void OnDrawOrderChanged()
        {
            if (DrawOrderChanged != null)
                DrawOrderChanged(this, EventArgs.Empty);
        }
    }
}
