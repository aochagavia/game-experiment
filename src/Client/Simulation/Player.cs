namespace Client.Simulation
{
    public class Player
    {
        public Direction Direction = Direction.East;
        public int WalkingStep = 0;
        public float X = 0;
        public float Y = 0;

        public bool IsWalking => WalkingStep != 0;
    }
}