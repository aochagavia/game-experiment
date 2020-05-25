using System;
using System.Collections.Generic;
using System.Linq;

namespace Client.Simulation
{
    public class InputState
    {
        private UInt64?[] _walkInputs = {null, null, null, null};

        public Direction? ActiveWalkInput()
        {
            var activeInput = _walkInputs
                .Select((ticks, i) => (ticks, i))
                .Where(selection => selection.ticks.HasValue)
                .OrderBy(selection => selection.ticks)
                .FirstOrDefault();

            var anyActiveInput = activeInput.ticks.HasValue;

            return anyActiveInput
                ? (Direction?) activeInput.i
                : null;
        }

        public void StartWalkInput(UInt64 ticks, Direction direction)
        {
            _walkInputs[(int) direction] = ticks;
        }

        public void StopWalkInput(Direction direction)
        {
            _walkInputs[(int) direction] = null;
        }
    }
}