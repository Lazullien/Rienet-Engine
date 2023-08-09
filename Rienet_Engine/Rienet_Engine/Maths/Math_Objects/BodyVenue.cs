namespace Rienet
{
    public interface IBodyVenue : IGameObject
    {
        public void OnCollision(PhysicsBody Target);
    }
}