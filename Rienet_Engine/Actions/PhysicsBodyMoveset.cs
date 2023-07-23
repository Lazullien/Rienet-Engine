using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Rienet
{
    public abstract class PhysicsBodyMoveset : IMoveset
    {
        public List<BodyState> AllStates;
        public Vector2 BasePos;
        public int StateIndex;
        public BodyState CurrentState;
        public StateBody stateBody;

        public ulong StartTime;
        public ulong LastUpdateTime;

        public bool DurationExists;
        public ulong Duration;
        public bool CooldownExists;
        public const ulong Cooldown = 30; //use arrow to override

        public ulong LastTimeUsed;
        public bool Invoked;

        protected GamePanel gp; //REMEMBER TO SET THIS TO A GAMEPANEL ON CREATION OF ANOTHER MOVESET

        public void SetBasePos(Vector2 basePos) => BasePos = basePos;

        public virtual void CheckInvocation()
        {
            if ((!CooldownExists) || gp.Time - LastTimeUsed >= Cooldown) Invoked = true;
        }

        public virtual void OnInvoke()
        {
            if (AllStates?.Count > 0)
            {
                CurrentState = AllStates[0];
                stateBody = CurrentState.body;
                StartTime = gp.Time;
                LastUpdateTime = gp.Time;
            }
        }

        public virtual void OnUpdate()
        {
            if (gp.Time - StartTime >= Duration || StateIndex >= AllStates.Count)
            {
                OnEnd();
                return;
            }

            if (StateIndex < AllStates.Count && gp.Time - LastUpdateTime > AllStates[StateIndex].StateDuration)
            {
                StateIndex++; LastUpdateTime = gp.Time;
            }

            if (StateIndex < AllStates.Count)
                CurrentState = AllStates[StateIndex];

            stateBody = CurrentState.body;

            //update the body with added velocity
            stateBody.SourcePos = BasePos;
            stateBody.VelocityIgnoringFriction = CurrentState.movement.Velocity;
            stateBody.Update();
        }

        public virtual void OnEnd() => LastTimeUsed = gp.Time;
    }
}