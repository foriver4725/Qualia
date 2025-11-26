namespace MyScripts.Runtime
{
    internal sealed class AnimalLeaveInvoker : MonoBehaviour
    {
        private Character possessingCharacter = null;

        // 現在憑依中かどうか
        internal bool IsPossessing => possessingCharacter != null;

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

            possessingCharacter.NameText.enabled = true;
            possessingCharacter.Collider.enabled = true;
            possessingCharacter.Renderer.enabled = true;
            possessingCharacter.Teleport(pc.transform.position, pc.transform.forward);
            possessingCharacter = null;
        }
    }
}
