namespace Rienet
{
    public interface IBehaviorScript
    {
        public void CheckState();
        public void SetState();
        public void Update();
    }

    public abstract class EntityBehavior : IBehaviorScript
    {
        IState CurrentState;

        public virtual void CheckState()
        {
        }

        public virtual void SetState()
        {
        }

        public virtual void Update()
        {
        }
    }
}