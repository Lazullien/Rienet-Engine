namespace Rienet
{
    public interface IState
    {
        public void Update();
        public void TryBreak();
    }
}