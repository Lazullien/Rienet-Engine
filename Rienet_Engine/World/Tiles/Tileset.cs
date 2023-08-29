using MonoGame.Extended.Sprites;

namespace Rienet
{
    public class Tileset
    {
        public int CurrentState;
        public Tile[] TileStates;
        public SpriteSheet Graphics;

        public Tileset(Tile[] TileStates, SpriteSheet Graphics, int ID)
        {
            foreach (Tile t in TileStates)
                t.ID = ID;

            this.TileStates = TileStates;
            this.Graphics = Graphics;
        }

        public Tile CurrentTileState
        {
            get { return TileStates[CurrentState]; }
            set { TileStates[CurrentState] = value; }
        }
    }
}