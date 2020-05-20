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
                WindowWidth = 960,
                WindowHeight = 540,
                WindowTitle = "Skeleton"
            });

            GraphicsDevice = VeldridStartup.CreateGraphicsDevice(Sdl2Window);
        }

        public void Dispose()
        {
            GraphicsDevice.Dispose();
        }
    }
}