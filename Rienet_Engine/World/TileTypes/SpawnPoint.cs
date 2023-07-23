using Microsoft.Xna.Framework;

namespace Rienet
{
    public class SpawnPoint : Tile
    {
        public SpawnPoint(Vector2 pos, Scene BelongedScene) : base(pos, BelongedScene)
        {
            ID = 3;
            body = new PhysicsBody(this, pos, new Vector2(1, 1), new() { new Hitbox(pos.X, pos.Y, 1, 1) }, 0, 0, false, BelongedScene);
        }

        public void OnNonCollidableCollision(PhysicsBody Target)
        {
            //set spawnpoint
            if (Target.BelongedObject is Entity e)
                SetSpawnPoint(e, this);
        }

        public static void SetSpawnPoint(Entity Target, SpawnPoint SP)
        {
            Target.Spawnpoint = SP.pos;
        }
    }

    public class SpawnPointSetter : Tile
    {
        SpawnPoint SP { get; set; }

        public SpawnPointSetter(Vector2 pos, Scene BelongedScene) : base(pos, BelongedScene)
        {
            ID = 4;
            body = new PhysicsBody(this, pos, new Vector2(1, 1), new() { new Hitbox(pos.X, pos.Y, 1, 1) }, 0, 0, false, BelongedScene);
        }

        public void SetAffiliatedSpawnPoint(SpawnPoint SP) => this.SP = SP;

        public void OnNonCollidableCollision(PhysicsBody Target)
        {
            //set spawnpoint
            if (Target.BelongedObject is Entity e)
                SpawnPoint.SetSpawnPoint(e, SP);
        }
    }
}