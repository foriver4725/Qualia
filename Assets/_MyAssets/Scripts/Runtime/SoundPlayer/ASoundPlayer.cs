namespace MyScripts.Runtime
{
    internal abstract class ASoundPlayer<TParam> : MonoBehaviour
        where TParam : ASSound
    {
        [SerializeField] private TParam param;
        [SerializeField, Tooltip("3D音源の場合、この場所からサウンドが再生される")] private Transform root;

        private protected TParam Param => param;
        private protected Transform Root => root;

        private protected abstract void Init();

        private void Awake()
        {
            Init();
        }
    }

    internal interface ISoundPlayerOptions { }

    // 最もよく使う
    internal abstract class ASoundPlayerWithType<TParam, TClipType> : ASoundPlayer<TParam>
        where TParam : ASSoundWithType<TClipType>
        where TClipType : Enum
    {
        private protected AudioSource[] AudioSources = null;

        private protected abstract byte TypeToByte(TClipType type);
        private protected abstract byte GetTypeAmount();

        internal virtual void LetPlay(TClipType type)
        {
            AudioClip clip = Param.GetClip(type);
            if (clip == null)
            {
                "No valid clip exists to play.".LogWarning();
                return;
            }

            AudioSources[TypeToByte(type)].LetPlay
            (
                clip,
                volume: Param.Volume
            );
        }

        private protected override void Init()
        {
            byte soundAmount = GetTypeAmount();

            AudioSources = new AudioSource[soundAmount];

            for (int i = 0; i < soundAmount; i++)
            {
                AudioSource source = Root.gameObject.AddComponent<AudioSource>();
                source.LetInit
                (
                    Param.Group
                );

                AudioSources[i] = source;
            }
        }
    }

    internal abstract class ASoundPlayerWithOptions<TParam, TOptions> : ASoundPlayer<TParam>
        where TParam : ASSound
        where TOptions : struct, ISoundPlayerOptions
    {
        internal abstract void LetPlay(TOptions options);
    }

    internal abstract class ASoundPlayerWithTypeAndOptions<TParam, TClipType, TOptions> : ASoundPlayer<TParam>
            where TParam : ASSoundWithType<TClipType>
            where TClipType : Enum
            where TOptions : struct, ISoundPlayerOptions
    {
        internal abstract void LetPlay(TClipType type, TOptions options);
    }
}
