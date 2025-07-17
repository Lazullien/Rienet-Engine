using System;
using System.Collections.Generic;

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
        /// <summary>
        /// first string is speaker, second is dialogue
        /// </summary>
        public Dictionary<string, string> dialogues;

        public void Check() => OnCheck();

        public void Start() => OnStart();

        public void Update() => OnUpdate();

        public void End() => OnEnd();

        static void PlaceHolder() {}
    }
}