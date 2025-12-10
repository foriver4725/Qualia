using MyScripts.Runtime.Log;

namespace MyScripts.Runtime
{
    internal sealed class AnimalLeaveInvoker : MonoBehaviour
    {
        [SerializeField] private Image displayImageBgBack;
        [SerializeField] private Image displayImageBg;
        [SerializeField] private Image displayImage;
        [SerializeField] private SAnima sAnima;
        [SerializeField] private SOSSoundPlayer soundPlayer;

        private Character possessingCharacter = null;

        // 現在憑依中かどうか
        internal bool IsPossessing => possessingCharacter != null;
        // 憑依中のキャラクターの種類 (憑依していないなら None)
        internal CharacterType PossessingCharacterType => (possessingCharacter != null) ? possessingCharacter.CharacterType : CharacterType.None;

        // 初めて取得したタイミングで true になり、以降二度と false にならない
        // 一回だけログで知らせるために使う
        private readonly Dictionary<CharacterType, bool> hasPossessedForTheFirstTimeTable = new()
        {
            { CharacterType.Land, false },
            { CharacterType.Sea, false },
            { CharacterType.Sky, false },
        };

        private void Awake()
        {
            UpdateDisplayImage(displayImage, displayImageBg, displayImageBgBack, possessingCharacter, sAnima);
            SetDisplayImageFillAmount(0.0f);
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

            UpdateDisplayImage(displayImage, displayImageBg, displayImageBgBack, possessingCharacter, sAnima);
            // TODO: SOSサインのサウンドを使いまわす!
            soundPlayer.LetPlay(SSOSSound.Situation.CouldRemove);

            // 初めて取得した場合、ログを表示する
            {
                CharacterType foundFirstTimeType = CharacterType.None;

                foreach (CharacterType type in hasPossessedForTheFirstTimeTable.Keys)
                {
                    if (type == CharacterType.None) continue;
                    if (type != character.CharacterType) continue;
                    if (hasPossessedForTheFirstTimeTable[type]) break;

                    foundFirstTimeType = type;
                    break;
                }

                hasPossessedForTheFirstTimeTable[foundFirstTimeType] = true;

                LogManager2.Instance.ShowAutomatically(foundFirstTimeType switch
                {
                    CharacterType.Land => "陸の移動速度が向上し、慣性ジャンプが可能になった！",
                    CharacterType.Sea => "水上の移動速度が大幅に向上した！",
                    CharacterType.Sky => "空中で再度ジャンプすると大ジャンプし、落下時に滑空するようになった！",
                    _ => ""
                });
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

            UpdateDisplayImage(displayImage, displayImageBg, displayImageBgBack, possessingCharacter, sAnima);
            // TODO: SOSサインのサウンドを使いまわす!
            soundPlayer.LetPlay(SSOSSound.Situation.CouldRemove);
        }

        internal void SetDisplayImageFillAmount(float amount)
        {
            amount = Mathf.Clamp01(amount);
            displayImageBg.fillAmount = amount;
        }

        private static void UpdateDisplayImage(Image image, Image bg, Image bgBack, Character possessingCharacter, SAnima sAnima)
        {
            if (possessingCharacter)
            {
                bgBack.enabled = true;
                bg.enabled = true;
                image.enabled = true;

                // 不透明にする
                Color color = possessingCharacter.MaterialColor;
                color.a = 1.0f;
                bg.color = color;

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
                bgBack.enabled = false;
                bg.enabled = false;
                image.enabled = false;
            }
        }
    }
}
