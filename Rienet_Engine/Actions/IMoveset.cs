using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics;

namespace Rienet
{
    public interface IMoveset
    {
        public void CheckInvocation();
        public void OnInvoke();
        public void OnUpdate();
        public void OnEnd(); //this method should not contain effects, it's only a cleanup

        public static void Replace(in IMoveset CurrentSet, out IMoveset ReplacedSet, IMoveset Moveset)
        {
            CurrentSet?.OnEnd();
            ReplacedSet = Moveset;
            ReplacedSet?.OnInvoke();
        }
    }
}