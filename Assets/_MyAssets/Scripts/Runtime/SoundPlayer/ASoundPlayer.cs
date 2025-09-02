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

    internal abstract class ASoundPlayerWithType<TParam, TClipType> : ASoundPlayer<TParam>
        where TParam : ASSoundWithType<TClipType>
        where TClipType : Enum
    {
        internal abstract void LetPlay(TClipType type);
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
