using MyScripts.SO.Reference;
using WalkSurface = MyScripts.SO.Reference.SWalkSound.Surface;
using WalkSoundLayer = MyScripts.Common.BorderLayer.WalkSound;

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
                source.playOnAwake = false;
                source.loop = false;
            }
        }

        /// <summary>
        /// プレイヤーが包含されている足音 Border のレイヤーを受け取り、そのレイヤーに対応する足音を鳴らす
        /// </summary>
        internal void LetPlay(byte layer)
        {
            WalkSurface surface = layer switch
            {
                _ when layer == WalkSoundLayer.Grass => WalkSurface.Grass,
                _ when layer == WalkSoundLayer.Sand => WalkSurface.Sand,
                _ when layer == WalkSoundLayer.Rock => WalkSurface.Rock,
                _ when layer == WalkSoundLayer.Water => WalkSurface.Water,
                _ => WalkSurface.Grass // default
            };

            AudioClip clip = walkSoundRef.GetClip(surface);

            //TODO: 音を鳴らす
        }
    }
}
