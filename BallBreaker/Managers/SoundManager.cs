using Microsoft.Xna.Framework.Audio;

namespace BallBreaker.Managers
{
    public class SoundManager
    {
        private SoundEffectInstance bounceSound;

        public SoundManager(SoundEffect bounce)
        {
            bounceSound = bounce.CreateInstance();
            bounceSound.Pitch = 0.75f;
            bounceSound.Volume = 0.1f;
        }

        public void PlayBounceSound()
        {
            if (bounceSound.State != SoundState.Playing)
                bounceSound.Play();
        }
    }
}
