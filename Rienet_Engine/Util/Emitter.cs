using System;
using Microsoft.Xna.Framework;

namespace Rienet
{
    /// <summary>
    /// For building maps, an object like this makes it simple to indicate what areas (or what other objects) can emit particles in a specific fashion
    /// </summary>
    public class ParticleEmitter
    {
        public Vector2 Pos { get; set; }
        public float Density { get; set; }
        public float Rate { get; set; }
        public float Magnitude { get; set; }
        public Action ActionOnEmit { get; set; } = delegate { };

        public void Emit()
        {
            ActionOnEmit();
        }
    }
}