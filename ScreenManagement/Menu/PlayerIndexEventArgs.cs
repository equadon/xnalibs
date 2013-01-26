using System;
using Microsoft.Xna.Framework;

namespace Valekhz.ScreenManagement.Menu
{
    /// <summary>
    /// Custom event argument which includes the index of the player who
    /// triggered the event. This is used by the MenuEntry.Selected event.
    /// </summary>
    public class PlayerIndexEventArgs : EventArgs
    {
        PlayerIndex _playerIndex;

        /// <summary>
        /// Constructor.
        /// </summary>
        public PlayerIndexEventArgs(PlayerIndex playerIndex)
        {
            this._playerIndex = playerIndex;
        }


        /// <summary>
        /// Gets the index of the player who triggered this event.
        /// </summary>
        public PlayerIndex PlayerIndex
        {
            get { return _playerIndex; }
        }
    }
}
