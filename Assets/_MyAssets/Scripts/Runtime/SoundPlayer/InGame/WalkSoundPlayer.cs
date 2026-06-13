namespace MyScripts.Runtime
{
    internal sealed class WalkSoundPlayer : ASoundPlayerWithTypeAndOptions<SWalkSound, SWalkSound.Surface, WalkSoundPlayer.Options>
    {
        internal struct Options : ISoundPlayerOptions
        {
            internal bool IsSprinting { get; init; }
        }
        

        private AudioSource[] audioSources = null;
        private MotionHandle[] fadeOutTweens = null;
        private bool[] arePlaying = null;

        private SWalkSound.Surface currentSurface = SWalkSound.Surface.None;
        private bool isCurrentSprinting = false;

        /// <summary>
        /// プレイヤーの地面の、更新通知を送る
        /// </summary>
        internal sealed override void LetPlayWwise(SWalkSound.Surface type, Options options)
        {
            if (type == currentSurface && options.IsSprinting == isCurrentSprinting) return;
            if (type == SWalkSound.Surface.None)
            {
                Param.Stop_Walk.Post(gameObject);
                currentSurface = type;
                return;
            } 
            Param.W_WalkPitch.SetValue(gameObject,options.IsSprinting ? 1f : 0f);
            isCurrentSprinting = options.IsSprinting;
            if(currentSurface != type)Param.Stop_Walk.Post(gameObject);

            Param.GetSwitch(type).SetValue(gameObject);
            if (currentSurface != type) Param.Play_Walk.Post(gameObject);
            currentSurface = type;
        }



        private protected sealed override void Init()
        {
            return; //今回は導入が出来るかの検証も兼ねているため複数ファイルの変更をしないために関数の削除ではなく、returnで止めている
            audioSources = new AudioSource[Param.MaxSoundAmount];
            fadeOutTweens = new MotionHandle[Param.MaxSoundAmount];
            arePlaying = new bool[Param.MaxSoundAmount];

            for (int i = 0; i < Param.MaxSoundAmount; i++)
            {
                AudioSource source = Root.gameObject.AddComponent<AudioSource>();
                source.LetInit
                (
                    Param.Group,
                    doLoop: true
                );

                audioSources[i] = source;
                fadeOutTweens[i] = default;
                arePlaying[i] = false;
            }
        }
    }
}
