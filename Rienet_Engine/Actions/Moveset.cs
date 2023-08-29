using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics;

namespace Rienet
{
    public abstract class Moveset
    {
        protected Entity TargetEntity;
        public bool HasDuration { get; private set; }
        public float Duration { get; private set; }

        public bool InMoveset { get; protected set; }
        public bool FirstTickInMoveset { get; protected set; }
        public float TimeSinceStarted { get; private set; }
        public float TimeSinceEnd { get; private set; }

        protected Moveset(Entity TargetEntity, bool HasDuration, float Duration)
        {
            this.TargetEntity = TargetEntity;
            this.HasDuration = HasDuration;
            this.Duration = Duration;

            FirstTickInMoveset = true;
        }

        public virtual void CheckInvocation()
        {
            if (InMoveset && FirstTickInMoveset) Replace(in TargetEntity.CurrentMoveSet, out TargetEntity.CurrentMoveSet, this);

            if (!InMoveset)
            {
                TimeSinceStarted = 0;
                TimeSinceEnd += GamePanel.ElapsedTime;
            }
        }

        public virtual void OnInvoke()
        {
            TimeSinceStarted = 0;
            FirstTickInMoveset = false;
        }

        public virtual void OnUpdate()
        {
            TimeSinceEnd = 0;

            if (!InMoveset || (HasDuration && TimeSinceStarted >= Duration))
            {
                OnEnd();
                return;
            }

            TimeSinceStarted += GamePanel.ElapsedTime;
        }

        public virtual void OnEnd()
        {
            FirstTickInMoveset = true;
            InMoveset = false;
            TargetEntity.CurrentMoveSet = null;
        }

        public static void Replace(in Moveset CurrentSet, out Moveset ReplacedSet, Moveset Moveset)
        {
            CurrentSet?.OnEnd();
            ReplacedSet = Moveset;
            ReplacedSet?.OnInvoke();
        }
    }
}