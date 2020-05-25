using System;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Client
{
    public class GameWindow : IDisposable
    {
        public Sdl2Window Sdl2Window { get; }
        public GraphicsDevice GraphicsDevice { get; }

        public GameWindow()
        {
            Sdl2Window = VeldridStartup.CreateWindow(new WindowCreateInfo
            {
                X = 100,
                Y = 100,
                WindowWidth = 540,
                WindowHeight = 540,
                WindowTitle = "Skeleton"
            });

            var gdOptions = new GraphicsDeviceOptions
			(
				debug: false,
				syncToVerticalBlank: true,
				swapchainDepthFormat: null
			);
            GraphicsDevice = VeldridStartup.CreateGraphicsDevice(Sdl2Window, gdOptions);
        }

        public void Dispose()
        {
            GraphicsDevice.Dispose();
        }
    }
}