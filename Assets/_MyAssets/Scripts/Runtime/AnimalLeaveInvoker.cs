using MyScripts.Runtime.Log;

namespace MyScripts.Runtime
{
    internal sealed class AnimalLeaveInvoker : MonoBehaviour
    {
        [SerializeField] private Image displayImage;
        [SerializeField] private SAnima sAnima;
        [SerializeField] private SOSSoundPlayer soundPlayer;

        private Character possessingCharacter = null;

        // 現在憑依中かどうか
        internal bool IsPossessing => possessingCharacter != null;
        // 憑依中のキャラクターの種類 (憑依していないなら None)
        internal CharacterType PossessingCharacterType => (possessingCharacter != null) ? possessingCharacter.CharacterType : CharacterType.None;

        // 初めて馬に憑依したタイミングで true になり、以降二度と false にならない
        // 馬になった時の能力強化を、一回だけログで知らせるために使う
        private bool hasPossessedHorseForTheFirstTime = false;

        private void Awake()
        {
            UpdateDisplayImage(displayImage, possessingCharacter, sAnima);
        }

        // キャラクターを取得する
        // キャラクターを見えなくする (当たり判定も無効化)
        internal void PossessCharacter(Character character)
        {
            if (possessingCharacter != null)
            {
                "すでに憑依中のキャラクターがあります。".Print(LogSettings.Error);
                return;
            }
            if (possessingCharacter == character)
            {
                "すでに憑依中のキャラクターを記録しようとしました。".Print(LogSettings.Error);
                return;
            }

            possessingCharacter = character;
            possessingCharacter.SetVisible(false);
            possessingCharacter.Collider.enabled = false;

            UpdateDisplayImage(displayImage, possessingCharacter, sAnima);
            // TODO: SOSサインのサウンドを使いまわす!
            soundPlayer.LetPlay(SSOSSound.Situation.CouldRemove);

            if (character.CharacterType == CharacterType.Land && !hasPossessedHorseForTheFirstTime)
            {
                hasPossessedHorseForTheFirstTime = true;

                LogManager2.Instance.ShowAutomatically("速度アップ、慣性ジャンプが可能になった！");
            }
        }

        // 憑依中のキャラクターから離脱する
        // キャラクターを見えるようにする (当たり判定も有効化)
        internal void LeaveCharacter(PlayerController pc)
        {
            if (possessingCharacter == null)
            {
                "憑依中のキャラクターがありません。".Print(LogSettings.Error);
                return;
            }

            possessingCharacter.SetVisible(true);
            possessingCharacter.Collider.enabled = true;
            possessingCharacter = null;

            UpdateDisplayImage(displayImage, possessingCharacter, sAnima);
            // TODO: SOSサインのサウンドを使いまわす!
            soundPlayer.LetPlay(SSOSSound.Situation.CouldRemove);
        }

        private static void UpdateDisplayImage(Image image, Character possessingCharacter, SAnima sAnima)
        {
            if (possessingCharacter)
            {
                image.enabled = true;

                image.sprite = possessingCharacter.CharacterType switch
                {
                    CharacterType.Land => sAnima.LandIcon,
                    CharacterType.Sea => sAnima.SeaIcon,
                    CharacterType.Sky => sAnima.SkyIcon,
                    _ => null
                };
            }
            else
            {
                image.enabled = false;
            }
        }
    }
}
