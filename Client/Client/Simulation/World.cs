using System;
using System.Collections.Generic;

namespace Client.Simulation
{
    public class World
    {
        private UInt64 _ticks = 0;

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
            if (_player.IsWalking)
            {
                return;
            }

            _player.Direction = direction;
            _player.WalkingStep = 1;
        }

        public void StopWalking()
        {
            // TODO: handle the case when multiple keys were pressed
            _player.WalkingStep = 0;
        }

        public void Tick()
        {
            // TODO: limit FPS in a smarter way
            _ticks++;
            if (_ticks % 128 != 0) return;

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

            _player.X = (_player.X + 0.01f * xMultiplier) % 960;
            _player.Y = (_player.Y + 0.01f * yMultiplier) % 540;
        }
    }
}