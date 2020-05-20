using Client.Simulation;
using Veldrid;

namespace Client
{
    class Program
    {
        static Direction? KeyToDirection(Key key)
        {
            return key switch
            {
                Key.Right => Direction.East,
                Key.Left => Direction.West,
                Key.Up => Direction.North,
                Key.Down => Direction.South,
                _ => null,
            };
        }

        static void Main(string[] args)
        {
            using var window = new GameWindow();
            using var worldRenderer = new WorldRenderer(window.GraphicsDevice);
            using var commandList = window.GraphicsDevice.ResourceFactory.CreateCommandList();

            var inputState = new InputState();
            var world = new World();
            world.Init();

            window.Sdl2Window.KeyDown += keyEvent =>
            {
                var direction = KeyToDirection(keyEvent.Key);
                if (direction != null)
                {
                    inputState.StartWalkInput(world.Ticks, direction.Value);
                }
            };

            window.Sdl2Window.KeyUp += keyEvent =>
            {
                var direction = KeyToDirection(keyEvent.Key);
                if (direction != null)
                {
                    inputState.StopWalkInput(direction.Value);
                }
            };

            while (window.Sdl2Window.Exists)
            {
                window.Sdl2Window.PumpEvents();
                world.ProcessInput(inputState);
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