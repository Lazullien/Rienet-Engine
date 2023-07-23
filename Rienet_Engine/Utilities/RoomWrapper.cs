namespace Rienet
{
    public class SceneWrapper
    {
        public int ID { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        //layers
        public int[,] behind { get; set; }
        public int[,] main { get; set; }
        public int[,] front { get; set; }
    }
}