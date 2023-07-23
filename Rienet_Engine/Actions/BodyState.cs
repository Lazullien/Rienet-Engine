namespace Rienet
{
    public struct BodyState
    {
        public ulong StateDuration;
        public StateBody body;
        public Movement movement;

        public BodyState(ulong StateDuration, StateBody body)
        {
            this.StateDuration = StateDuration;
            this.body = body;
            movement = default;
        }

        public BodyState(ulong StateDuration, StateBody body, Movement movement)
        {
            this.StateDuration = StateDuration;
            this.body = body;
            this.movement = movement;
        }
    }
}