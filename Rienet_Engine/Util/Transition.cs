using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Rienet
{
    public class Transition : PhysicsBody
    {
        public Hitbox Area;
        public int R1;
        public Vector2 SpawnPosInR1;
        public Action<Entity> PreTransition;
        public Action<Entity> OnTransition;
        public Action<Entity> PostTransition;

        public Transition(Hitbox Area, int R1, Vector2 SpawnPosInR1, Scene scene) : base(null, Area.pos, Area.size, new() { Area }, 0, 0, false, scene)
        {
            this.Area = Area;
            this.R1 = R1;
            this.SpawnPosInR1 = SpawnPosInR1;
            //default
            OnTransition = Transit;
        }

        public override void OnNonCollidableCollision(PhysicsBody Target)
        {
            //send target and container entity of target elsewhere (non entities will not be transitted)
            if (Target.BelongedObject is Entity e)
                PreTransition?.Invoke(e);
        }

        public void Transit(Entity e)
        {
            Scene TargetScene = BelongedScene.world.Scenes[R1];
            Vector2 TargetPos = SpawnPosInR1;
            e.SetScene(TargetScene);
            e.body.pos = TargetPos;
            Camera cam = GamePanel.cam;
            if (cam.LockOn && e == cam.LockOnEntity)
                cam.Scene = TargetScene;
            PostTransition?.Invoke(e);
        }
    }
}