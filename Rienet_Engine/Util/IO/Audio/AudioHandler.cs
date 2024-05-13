using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

//LATER IMPLEMENT MORE COMPLEX AUDIO FUNCTIONS WITH AUDIOENGINE AND STUFF HERE
namespace Rienet
{
    public class AudioHandler
    {
        /// <summary>
        /// Overall volume, can have relative highs and lows
        /// </summary>
        public static float MasterVolume { get; set; } = 0.2f;

        /// <summary>
        /// A place for storing audiocomponents, most of these ac are only temporarily stored here as they take up a lot of ram
        /// </summary>
        public static Dictionary<int, AudioComponent> AudioComponents { get; private set; } = new();
        public static List<AudioComponent> AudiosInPlay { get; private set; } = new();

        public static void SetMasterVolume(float masterVolume) => MasterVolume = masterVolume;

        public static void LoadAudio(string file, int ID, bool Looping, out AudioComponent audioComponent)
        {
            AudioComponent ac = new(file, GamePanel.content, Looping);
            audioComponent = ac;
            AudioComponents.Add(ID, ac);
        }

        public static bool IsPlaying(int id, out AudioComponent ac)
        {
            return AudioComponents.TryGetValue(id, out ac) && AudiosInPlay.Contains(ac);
        }

        public static void Play(int ac)
        {
            if (AudioComponents.ContainsKey(ac))
            {
                var a = AudioComponents[ac];
                a.SetVolume(MasterVolume);
                a.OnPlay();
            }
        }

        public static void Update(GameTime gameTime)
        {
            for (int i = 0; i < AudiosInPlay.Count; i++)
                AudiosInPlay[i].Update(gameTime);
        }

        public static void PauseAll()
        {
            for (int i = 0; i < AudiosInPlay.Count; i++)
                AudiosInPlay[i].Pause();
        }

        public static void ResumeAll()
        {
            foreach (AudioComponent a in AudioComponents.Values)
                a.Resume();
        }

        public static void StopAll()
        {
            for (int i = 0; i < AudiosInPlay.Count; i++)
                AudiosInPlay[i].Stop();
        }
    }
}