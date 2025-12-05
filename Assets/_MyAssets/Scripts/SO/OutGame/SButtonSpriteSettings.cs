using MyScripts.Common.Button;

namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_ButtonSpriteSettings", menuName = "SO/OutGame/Button Sprite Settings")]
    internal sealed class SButtonSpriteSettings : ScriptableObject
    {
        internal enum ButtonType : byte
        {
            Normal,
            SaveSlot,
        }

        [SerializeField] private SpriteSettings normal;
        [SerializeField] private SpriteSettings saveSlot;

        internal SpriteSettings Get(ButtonType type) => type switch
        {
            ButtonType.Normal => normal,
            ButtonType.SaveSlot => saveSlot,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
