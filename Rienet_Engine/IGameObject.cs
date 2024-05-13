namespace Rienet
{
    public interface IGameObject
    {
        public void OnCreation();
        public void Update();
        public void OnDestruction();
    }
}