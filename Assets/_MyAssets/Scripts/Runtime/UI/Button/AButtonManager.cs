using MyScripts.Common.Button;

namespace MyScripts.Runtime.UI.Button
{
    internal abstract class AButtonManager : ATextButtonManager
    {
        [SerializeField] private SSoundSetting soundSetting;
        [SerializeField] private ButtonSoundPlayer soundPlayer;

        private protected sealed override void PlayHoverSe()
        {
            if (soundSetting.DoesPlayButtonHoverSe)
                soundPlayer.LetPlay(SButtonSound.Action.Hover);
        }

        private protected sealed override void PlayClickSe()
        {
            soundPlayer.LetPlay(SButtonSound.Action.Click);
        }
    }
}
