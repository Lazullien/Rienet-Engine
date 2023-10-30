using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Rienet
{
    public class Transition : PhysicsBody
    {
        public Hitbox Area;
        public int R1;
        public Vector2 SpawnPosInR1;
        public int R2;
        public Vector2 SpawnPosInR2;

        public Transition(Hitbox Area, int R1, Vector2 SpawnPosInR1, int R2, Vector2 SpawnPosInR2, Scene scene) : base(null, Area.pos, Area.size, new() { Area }, 0, 0, false, scene)
        {
            this.Area = Area;
            this.R1 = R1;
            this.SpawnPosInR1 = SpawnPosInR1;
            this.R2 = R2;
            this.SpawnPosInR2 = SpawnPosInR2;
        }

        public override void OnNonCollidableCollision(PhysicsBody Target, Vector2 Intersection)
        {
            //send target and container entity of target elsewhere (non entities will not be transitted)
            if (Target.BelongedObject is Entity e)
                Transit(e);
        }

        public void Transit(Entity e)
        {
            //call spawn from corresponding room (R1)
            bool SceneIsR1 = BelongedScene.ID == R1;
            Scene TargetScene = BelongedScene.world.Scenes[SceneIsR1 ? R2 : R1];
            Vector2 TargetPos = SceneIsR1 ? SpawnPosInR2 : SpawnPosInR1;
            e.SetScene(TargetScene);
            e.body.pos = TargetPos;
            Camera cam = GamePanel.cam;
            if (cam.LockOn && e == cam.LockOnEntity)
                cam.Scene = TargetScene;
        }
    }
}