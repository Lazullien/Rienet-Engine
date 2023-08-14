namespace Rienet
{
    public interface IBehaviorScript
    {
        public void CheckState();
        public void Update();
        public void End();
    }
}