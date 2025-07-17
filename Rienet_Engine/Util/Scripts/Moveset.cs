using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Rienet
{
    /** Basically a state machine **/
    public abstract class Moveset
    {
        protected Entity TargetEntity;
        public bool HasDuration { get; private set; }
        public float Duration { get; protected set; }

        public bool InMoveset { get; protected set; }
        public bool FirstTickInMoveset { get; protected set; }
        public float TimeSinceStarted { get; protected set; }
        public float TimeSinceEnd { get; set; }

        public Action Invoke { get; set; } = delegate { };
        public Action Update { get; set; } = delegate { };
        public Action End { get; set; } = delegate { };

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
            InMoveset = true;
            TimeSinceStarted = 0;
            FirstTickInMoveset = false;
            Invoke();
        }

        public virtual void OnUpdate()
        {
            Update();

            TimeSinceEnd = 0;

            if ((!InMoveset) || (HasDuration && TimeSinceStarted >= Duration))
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

            End();
        }

        public static void Replace(in Moveset CurrentSet, out Moveset ReplacedSet, Moveset Moveset)
        {
            CurrentSet?.OnEnd();
            ReplacedSet = Moveset;
            ReplacedSet?.OnInvoke();
        }
    }
}