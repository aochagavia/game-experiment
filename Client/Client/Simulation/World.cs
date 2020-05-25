using System;
using System.Collections.Generic;

namespace Client.Simulation
{
    public class World
    {
        public UInt64 Ticks { get; private set; } = 0;

        private readonly List<Player> _players = new List<Player>();
        public IEnumerable<Player> Players => _players;

        private Player _player => _players[0];

        public void Init()
        {
            // TODO: move this somewhere else
            _players.Add(new Player());
        }

        public void StartWalking(Direction direction)
        {
            if (_player.IsWalking && _player.Direction == direction)
            {
                return;
            }

            _player.Direction = direction;
            _player.WalkingStep = 1;
        }

        public void StopWalking()
        {
            _player.WalkingStep = 0;
        }

        public void ProcessInput(InputState inputState)
        {
            var direction = inputState.ActiveWalkInput();
            if (direction.HasValue)
            {
                StartWalking(direction.Value);
            }
            else
            {
                StopWalking();
            }
        }

        public void Tick()
        {
            Ticks++;
            if (Ticks % 2 != 0)
            {
                return;
            }

            if (!_player.IsWalking) return;

            _player.WalkingStep = (_player.WalkingStep + 1) % 8;
            if (_player.WalkingStep == 0)
            {
                _player.WalkingStep++;
            }

            var xMultiplier = _player.Direction switch
            {
                Direction.East => 1f,
                Direction.West => -1f,
                _ => 0f,
            };

            var yMultiplier = _player.Direction switch
            {
                Direction.North => 1f,
                Direction.South => -1f,
                _ => 0f,
            };

            _player.X = (_player.X + 0.01f * xMultiplier) % 540;
            _player.Y = (_player.Y + 0.01f * yMultiplier) % 540;
        }
    }
}
