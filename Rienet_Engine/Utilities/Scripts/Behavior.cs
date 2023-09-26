using System;

namespace Rienet
{
    public interface BehaviorScript
    {
        public void CheckState();
        public void Update();
        public void End();
    }
}