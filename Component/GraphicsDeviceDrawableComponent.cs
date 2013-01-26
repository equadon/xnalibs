using System;
using Microsoft.Xna.Framework.Graphics;

namespace Valekhz.Components
{

    /// <summary>
    ///   Lightweight variant DrawableGameComponent that doesn't reference the Game class
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     This variant of the DrawableComponent class stores a graphics device and
    ///     calls the LoadContent() method at the appropriate time. It is useful
    ///     if the deriving class actually makes direct use of the graphics device.
    ///   </para>
    ///   <para>
    ///     To work, this class requires to things: A GameServices collection and
    ///     an entry for the IGraphicsDeviceService. You can easily implement this
    ///     interface yourself for any custom graphics device manager.
    ///   </para>
    /// </remarks>
    public class GraphicsDeviceDrawableComponent : DrawableComponent, IDisposable
    {
        #region Fields

        /// <summary>XNA game service provider (can be null)</summary>
        /// <remarks>
        ///   This is only set when the component is initialized using the IServiceProvider
        ///   constructor, where it needs to remember the service provider until the
        ///   Initialize() method has been called.
        /// </remarks>
        private IServiceProvider _serviceProvider;

        /// <summary>Graphics device service this component is bound to.</summary>
        private IGraphicsDeviceService _graphicsDeviceService;

        #endregion

        #region Properties

        /// <summary>GraphicsDevice this component is bound to. Can be null.</summary>
        public GraphicsDevice GraphicsDevice
        {
            get { return _graphicsDeviceService.GraphicsDevice; }
        }

        public IServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
        }

        #endregion

        #region Constructors

        /// <summary>Initializes a new drawable component.</summary>
        /// <param name="serviceProvider">
        ///   Service provider from which the graphics device service will be taken
        /// </param>
        public GraphicsDeviceDrawableComponent(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            // We do not look up the graphics device service right here because it might
            // not exist yet. XNA uses a two-stage initialization to avoid initialization
            // order dependencies. When constructed, all components add their own services
            // and only when Initialize() is called do they look up other services they need.
        }

        /// <summary>Initializes a new drawable component</summary>
        /// <param name="graphicsDeviceService">
        ///   Graphics device service the component will use
        /// </param>
        /// <remarks>
        ///   This constructor is mainly relevant for users of IoC containers which
        ///   can wire up components to each other automatically. For the XNA
        ///   game services model, the service provider-based constructor should
        ///   be used instead because it uses a two-stage initialization process
        ///   where components wire up to each other in <see cref=" Initialize" />.
        /// </remarks>
        public GraphicsDeviceDrawableComponent(IGraphicsDeviceService graphicsDeviceService)
        {
            _graphicsDeviceService = graphicsDeviceService;
        }

        #endregion

        #region Implementation of IDrawable

        /// <summary>Immediately releases all resources owned by this instance</summary>
        /// <remarks>
        ///   This method is not suitable for being called during a GC run, it is intended
        ///   for manual usage when you actually want to get rid of the drawable component.
        /// </remarks>
        public virtual void Dispose()
        {
            // Unsubscribe from the events of the graphics device service only once
            if (_graphicsDeviceService != null)
            {
                UnsubscribeFromGraphicsDeviceService();
                _graphicsDeviceService = null;
            }

            _serviceProvider = null;
        }

        #endregion

        /// <summary>Gives the game component a chance to initialize itself</summary>
        public override void Initialize()
        {
            // Only do something here if we were initialized with a service provider,
            // meaning that the developer is using XNA's game services system instead
            // of a full-blown IoC container.
            if (_graphicsDeviceService == null)
            {
                // Look for the graphics device service in the game's service container
                _graphicsDeviceService = _serviceProvider.GetService(
                  typeof(IGraphicsDeviceService)
                ) as IGraphicsDeviceService;

                // Like our XNA pendant, we absolutely require the graphics device service
                if (_graphicsDeviceService == null)
                    throw new InvalidOperationException("Graphics device service not found");
            }

            // Done, now we can register to the graphics device service's events
            SubscribeToGraphicsDeviceService();
        }

        /// <summary>
        ///   Called when graphics resources need to be loaded. Override this method to load
        ///   any game-specific graphics resources.
        /// </summary>
        protected virtual void LoadContent() { }

        /// <summary>
        ///   Called when graphics resources need to be unloaded. Override this method to unload
        ///   any game-specific graphics resources.
        /// </summary>
        protected virtual void UnloadContent() { }

        /// <summary>
        ///   Subscribes this component to the events of the graphics device service.
        /// </summary>
        private void SubscribeToGraphicsDeviceService()
        {
            // Register to the events of the graphics device service so we know when
            // the graphics device is set up, shut down or reset.
            _graphicsDeviceService.DeviceCreated += new EventHandler<EventArgs>(DeviceCreated);
            _graphicsDeviceService.DeviceResetting += new EventHandler<EventArgs>(DeviceResetting);
            _graphicsDeviceService.DeviceReset += new EventHandler<EventArgs>(DeviceReset);
            _graphicsDeviceService.DeviceDisposing += new EventHandler<EventArgs>(DeviceDisposing);

            // If a graphics device has already been created, we need to simulate the
            // DeviceCreated event that we did miss because we weren't born yet :)
            if (_graphicsDeviceService.GraphicsDevice != null)
            {
                LoadContent();
            }
        }

        /// <summary>
        ///   Unsubscribes this component from the events of the graphics device service.
        /// </summary>
        private void UnsubscribeFromGraphicsDeviceService()
        {
            // Unsubscribe from the events again
            _graphicsDeviceService.DeviceCreated -= new EventHandler<EventArgs>(DeviceCreated);
            _graphicsDeviceService.DeviceResetting -= new EventHandler<EventArgs>(DeviceResetting);
            _graphicsDeviceService.DeviceReset -= new EventHandler<EventArgs>(DeviceReset);
            _graphicsDeviceService.DeviceDisposing -= new EventHandler<EventArgs>(DeviceDisposing);

            // If the graphics device is still active, we give the component a chance
            // to clean up its data
            if (_graphicsDeviceService.GraphicsDevice != null)
            {
                UnloadContent();
            }
        }

        /// <summary>Called when the graphics device is created</summary>
        /// <param name="sender">Graphics device service that created a new device</param>
        /// <param name="arguments">Not used</param>
        private void DeviceCreated(object sender, EventArgs arguments)
        {
            LoadContent();
        }

        /// <summary>Called before the graphics device is being reset</summary>
        /// <param name="sender">Graphics device service that is resetting its device</param>
        /// <param name="arguments">Not used</param>
        private void DeviceResetting(object sender, EventArgs arguments)
        {
        }

        /// <summary>Called after the graphics device has been reset</summary>
        /// <param name="sender">Graphics device service that has just reset its device</param>
        /// <param name="arguments">Not used</param>
        private void DeviceReset(object sender, EventArgs arguments)
        {
        }

        /// <summary>Called before the graphics device is being disposed</summary>
        /// <param name="sender">Graphics device service that's disposing the device</param>
        /// <param name="arguments">Not used</param>
        private void DeviceDisposing(object sender, EventArgs arguments)
        {
            UnloadContent();
        }
    }
}