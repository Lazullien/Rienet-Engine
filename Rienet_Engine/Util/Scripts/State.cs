namespace Rienet
{
    /** A more abstract version of moveset state machine **/
    public interface IState
    {
        public void Check();
        public void Start();
        public void Update();
        public void End();
    }
}