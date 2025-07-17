using UnityEngine;

namespace Xuf.Common
{
    public class AudioManager : CMonoSingleton<AudioManager>
    {
        public AudioSource bgSrc;
        public AudioSource fxSrc;

        protected override void Awake()
        {
            base.Awake();
            bgSrc = gameObject.AddComponent<AudioSource>();
            bgSrc.volume = 0.2f;
            fxSrc = gameObject.AddComponent<AudioSource>(); ;
            fxSrc.volume = 0.2f;
        }

    }
}
