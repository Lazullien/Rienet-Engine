using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Rienet
{
    /// <summary>
    /// All particles are concentratedly handled here, their drawing only occurs in camera-existant scenes, and are not handled by these scenes
    /// </summary>
    public class ParticleEngine
    {
        //all handled here centrally, only drawing in camera existing scenes
        public static List<Particle> Particles { get; private set; } = new();

        //updates even without being seen (for easier killing)
        public static void Update()
        {
            for (int i = 0; i < Particles.Count; i++)
                Particles[i].Update();
        }
    }
}