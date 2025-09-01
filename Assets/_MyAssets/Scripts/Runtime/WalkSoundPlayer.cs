using MyScripts.SO.Reference;

namespace MyScripts.Runtime
{
    internal sealed class WalkSoundPlayer : MonoBehaviour
    {
        [SerializeField] private SWalkSound walkSoundRef;
        [SerializeField] private Transform walkSoundRoot;
        [SerializeField, Range(1, 8), Tooltip("同時に鳴る足音の最大数")] private byte maxSoundAmount = 4;

        private AudioSource[] audioSources = null;
        private bool[] arePlaying = null;
        private int headIndex = 0; // 順番にサウンドを鳴らし、まだ鳴っていないものの先頭を指し示す

        private SWalkSound.Surface currentSurface = SWalkSound.Surface.None;

        private void Awake()
        {
            InitializeAudioSources();
        }

        /// <summary>
        /// プレイヤーの地面の、更新通知を送る
        /// </summary>
        internal void LetPlay(SWalkSound.Surface surface)
        {
            if (surface == currentSurface) return;
            currentSurface = surface;

            // TODO: これらの非同期処理が混線すると？
            if (currentSurface == SWalkSound.Surface.None)
            {
                // 現在鳴っているサウンドをフェードアウトして止める

                for (int i = 0; i < (headIndex - 1); i++)
                {
                    audioSources[i]
                        .DOFade(0.0f, 0.2f)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() =>
                        {
                            audioSources[i].Stop();
                            audioSources[i].clip = null;
                            arePlaying[i] = false;

                            headIndex = 0;
                        });
                }
            }
            else
            {
                // 新規にサウンドの再生を始める

                AudioClip clip = walkSoundRef.GetClip(currentSurface);
                if (clip == null)
                {
                    "Failed to get walk sound clip.".LogWarning();
                    return;
                }

                audioSources[headIndex].clip = clip;
                audioSources[headIndex].Play();

                headIndex++;
            }
        }

        private void InitializeAudioSources()
        {
            audioSources = new AudioSource[maxSoundAmount];
            arePlaying = new bool[maxSoundAmount];

            for (int i = 0; i < maxSoundAmount; i++)
            {
                AudioSource source = walkSoundRoot.gameObject.AddComponent<AudioSource>();
                source.outputAudioMixerGroup = walkSoundRef.Group;
                source.playOnAwake = false;
                source.loop = true;

                audioSources[i] = source;
                arePlaying[i] = false;
            }

            headIndex = 0;
        }
    }
}
