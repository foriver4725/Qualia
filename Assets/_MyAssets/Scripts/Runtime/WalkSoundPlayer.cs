using MyScripts.SO.Reference;

namespace MyScripts.Runtime
{
    internal sealed class WalkSoundPlayer : MonoBehaviour
    {
        [SerializeField] private SWalkSound walkSoundRef;
        [SerializeField] private Transform walkSoundRoot;
        [SerializeField, Range(1, 8), Tooltip("同時に鳴る足音の最大数")] private byte maxSoundAmount = 4;

        private AudioSource[] audioSources = null;

        private void Awake()
        {
            for (int i = 0; i < maxSoundAmount; i++)
            {
                AudioSource source = walkSoundRoot.gameObject.AddComponent<AudioSource>();
                source.outputAudioMixerGroup = walkSoundRef.Group;
                source.playOnAwake = false;
                source.loop = false;
            }
        }

        /// <summary>
        /// プレイヤーがいる地面に対応する足音を鳴らす
        /// </summary>
        internal void LetPlay(SWalkSound.Surface surface)
        {
            AudioClip clip = walkSoundRef.GetClip(surface);

            //TODO: 音を鳴らす/止める
        }
    }
}
