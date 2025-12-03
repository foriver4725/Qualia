using MyScripts.Runtime.Log;

namespace MyScripts.Runtime
{
    internal sealed class AnimalLeaveInvoker : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI displayText;
        [SerializeField] private PlayerController pc; // 地面に設置していないと離脱できないので、それを取得するためだけの参照 (循環参照だけど...)

        private Character possessingCharacter = null;

        // 現在憑依中かどうか
        internal bool IsPossessing => possessingCharacter != null;
        // 憑依中のキャラクターの種類 (憑依していないなら None)
        internal CharacterType PossessingCharacterType => (possessingCharacter != null) ? possessingCharacter.CharacterType : CharacterType.None;

        private void Awake()
        {
            UpdateDisplayText(displayText, possessingCharacter);
        }

        // キャラクターに憑依する
        // キャラクターを見えなくする (名前も消す、当たり判定も無効化)
        // キャラクターの位置にプレイヤーをテレポートさせる
        internal void PossessCharacter(PlayerController pc, Character character)
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

            character.NameText.enabled = false;
            character.Collider.enabled = false;
            character.Renderer.enabled = false;
            pc.Teleport(character.transform.position, character.transform.forward);

            UpdateDisplayText(displayText, possessingCharacter);
        }

        // 憑依中のキャラクターから離脱する
        // キャラクターを見えるようにする (名前も表示する、当たり判定も有効化)
        // プレイヤーの位置にキャラクターをテレポートさせる
        internal void LeaveCharacter(PlayerController pc)
        {
            if (possessingCharacter == null)
            {
                "憑依中のキャラクターがありません。".Print(LogSettings.Error);
                return;
            }

            // 地面に設置していないとダメ
            if (!pc.IsGrounded)
            {
                LogManager2.Instance.ShowAutomatically("地面に設置していないと、離脱できません", duration: 1.0f, fadeoutDuration: 0.5f);
                return;
            }

            possessingCharacter.NameText.enabled = true;
            possessingCharacter.Collider.enabled = true;
            possessingCharacter.Renderer.enabled = true;
            {
                // 見えるように、少し前の位置にテレポートさせる
                // 数値は決め打ち
                Vector3 teleportPosition = pc.transform.position + pc.transform.forward * 2.0f;
                possessingCharacter.Teleport(teleportPosition, pc.transform.forward);
            }
            possessingCharacter = null;

            UpdateDisplayText(displayText, possessingCharacter);
        }

        private static void UpdateDisplayText(TextMeshProUGUI text, Character possessingCharacter)
        {
            string currentCharacterString = possessingCharacter switch
            {
                null => "人間",
                _ => possessingCharacter.CharacterType switch
                {
                    CharacterType.Horse => "馬",
                    _ => "???",
                },
            };

            text.SetTextFormat("現在 : {0}", currentCharacterString);
        }
    }
}
