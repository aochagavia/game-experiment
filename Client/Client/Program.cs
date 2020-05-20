using Client.Simulation;
using Veldrid;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            using var window = new GameWindow();
            using var worldRenderer = new WorldRenderer(window.GraphicsDevice);
            using var commandList = window.GraphicsDevice.ResourceFactory.CreateCommandList();

            var world = new World();
            world.Init();

            window.Sdl2Window.KeyDown += keyEvent =>
            {
                switch (keyEvent.Key)
                {
                    case Key.Right:
                        world.StartWalking(Direction.East);
                        break;
                    case Key.Left:
                        world.StartWalking(Direction.West);
                        break;
                    case Key.Up:
                        world.StartWalking(Direction.North);
                        break;
                    case Key.Down:
                        world.StartWalking(Direction.South);
                        break;
                }
            };

            window.Sdl2Window.KeyUp += keyEvent =>
            {
                // TODO: only stop walking when you release the key for your own direction
                world.StopWalking();
            };

            while (window.Sdl2Window.Exists)
            {
                window.Sdl2Window.PumpEvents();
                world.Tick();

                // Draw
                commandList.Begin();
                worldRenderer.Draw(commandList, world);
                commandList.End();

                window.GraphicsDevice.SubmitCommands(commandList);
                window.GraphicsDevice.SwapBuffers();
                window.GraphicsDevice.WaitForIdle();
            }
        }
    }
}