using System;
using Microsoft.Xna.Framework;

namespace ValekhzLibs.Components
{
    /// <summary>
    ///   Variant of the XNA GameComponent that doesn't reference the Game class
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     This is a lightweight version of GameComponent that can be used without
    ///     requiring a Game class to be present. Useful to get all the advantages
    ///     of the XNA GameServices architecture even when you have initialized and
    ///     manage the graphics device yourself.
    ///   </para>
    ///   <para>
    ///     The name of this class is the same as 'GameComponent' minus the 'Game' part
    ///     as the Game reference is what this class removes from its namesake.
    ///   </para>
    /// </remarks>
    public class Component : IGameComponent, IUpdateable
    {
        /// <summary>Triggered when the value of the enabled property is changed.</summary>
        public event EventHandler<EventArgs> EnabledChanged;

        /// <summary>Triggered when the value of the update order property is changed.</summary>
        public event EventHandler<EventArgs> UpdateOrderChanged;

        #region Properties

        private bool _enabled;
        private int _updateOrder;

        /// <summary>
        /// True when the updateable component is enabled and should be updated.
        /// </summary>
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (value != _enabled)
                {
                    _enabled = value;
                    OnEnabledChanged();
                }
            }
        }

        /// <summary>
        /// Indicates when the updateable component should be updated in relation
        /// to other updateables. Has ano effect by itself.
        /// </summary>
        public int UpdateOrder
        {
            get { return _updateOrder; }
            set
            {
                if (value != _updateOrder)
                {
                    _updateOrder = value;
                    OnUpdateOrderChanged();
                }
            }
        }

        #endregion

        public Component()
        {
            _enabled = true;
        }

        #region Implementation of IGameComponent

        /// <summary>Gives the game component a chance to initialize itself</summary>
        public virtual void Initialize() { }

        #endregion

        #region Implementation of IUpdateable

        /// <summary>Called when the component needs to update its state.</summary>
        /// <param name="gameTime">Provides a snapshot of the Game's timing values</param>
        public virtual void Update(GameTime gameTime) { }

        #endregion

        protected virtual void OnEnabledChanged()
        {
            if (EnabledChanged != null)
                EnabledChanged(this, EventArgs.Empty);
        }

        protected virtual void OnUpdateOrderChanged()
        {
            if (UpdateOrderChanged != null)
                UpdateOrderChanged(this, EventArgs.Empty);
        }
    }
}
