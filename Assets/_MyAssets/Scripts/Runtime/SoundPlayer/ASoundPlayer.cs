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
    internal abstract class ASoundPlayerWithType<TParam, TClipTypeByte> : ASoundPlayer<TParam>
        where TParam : ASSoundWithType<TClipTypeByte>
        where TClipTypeByte : struct, Enum
    {
        private Array typeValues = null;
        private protected AudioSource[] AudioSources = null;

        internal virtual void LetPlay(TClipTypeByte type)
        {
            AudioClip clip = Param.GetClip(type);
            if (clip == null)
            {
                "No valid clip exists to play.".Print(LogSettings.Warning);
                return;
            }

            AudioSources[type.ToInteger<TClipTypeByte, byte>()].LetPlay
            (
                clip,
                volume: Param.Volume
            );
        }

        private protected byte TypeCount
        {
            get
            {
                typeValues ??= Enum.GetValues(typeof(TClipTypeByte));
                return (byte)typeValues.Length;
            }
        }

        private protected override void Init()
        {
            AudioSources = new AudioSource[TypeCount];

            for (int i = 0; i < TypeCount; i++)
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

    internal abstract class ASoundPlayerWithTypeAndOptions<TParam, TClipTypeByte, TOptions> : ASoundPlayer<TParam>
            where TParam : ASSoundWithType<TClipTypeByte>
            where TClipTypeByte : struct, Enum
            where TOptions : struct, ISoundPlayerOptions
    {
        internal abstract void LetPlay(TClipTypeByte type, TOptions options);
    }
}
