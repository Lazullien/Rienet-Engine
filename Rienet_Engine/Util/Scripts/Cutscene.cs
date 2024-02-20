using System;

namespace Rienet
{
    public class Cutscene : IState
    {
        public int id;
        public int activationScene;
        public bool repeatable;
        public bool played;
        public Action OnCheck = PlaceHolder;
        public Action OnStart = PlaceHolder;
        public Action OnUpdate = PlaceHolder;
        public Action OnEnd = PlaceHolder;

        public void Check() => OnCheck();

        public void Start() => OnStart();

        public void Update() => OnUpdate();

        public void End() => OnEnd();

        static void PlaceHolder() {}
    }
}