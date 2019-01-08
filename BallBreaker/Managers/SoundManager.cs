using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using BallBreaker.HelperObjects;

namespace BallBreaker.Managers
{
    public enum Sounds
    {
        Bounce,
        Slide,
        Success
    }

    public class SoundManager
    {
        private Dictionary<Sounds, SoundInfo> soundEffects;
        
        public void PlaySound(Sounds sound)
        {
            if (!soundEffects.ContainsKey(sound))
                return;

            if (soundEffects[sound].SoundEffectInstance.State != SoundState.Playing)
                soundEffects[sound].SoundEffectInstance.Play();
        }

        public void LoadContent(ContentManager content)
        {
            soundEffects = new Dictionary<Sounds, SoundInfo>();

            var soundEffect = content.Load<SoundEffect>("Sound/Bounce");
            var soundEffectInstance = soundEffect.CreateInstance();
            soundEffectInstance.Volume = 0.05f;
            soundEffectInstance.Pitch = 0.75f;
            soundEffects.Add(
                Sounds.Bounce,
                new SoundInfo()
                {
                    SoundEffect = soundEffect,
                    SoundEffectInstance = soundEffectInstance,
                });

            soundEffect = content.Load<SoundEffect>("Sound/Slide");
            soundEffectInstance = soundEffect.CreateInstance();
            soundEffectInstance.Volume = 1f;
            soundEffectInstance.Pitch = 0.75f;
            soundEffects.Add(
                Sounds.Slide,
                new SoundInfo()
                {
                    SoundEffect = soundEffect,
                    SoundEffectInstance = soundEffectInstance
                });

            soundEffect = content.Load<SoundEffect>("Sound/Success");
            soundEffectInstance = soundEffect.CreateInstance();
            soundEffectInstance.Volume = 0.2f;
            soundEffectInstance.Pitch = 0.5f;
            soundEffects.Add(
                Sounds.Success,
                new SoundInfo()
                {
                    SoundEffect = soundEffect,
                    SoundEffectInstance = soundEffectInstance
                });
        }
    }
}
