using MyScripts.Common.Button;

namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_ButtonSpriteSettings", menuName = "SO/OutGame/Button Sprite Settings")]
    internal sealed class SButtonSpriteSettings : ScriptableObject
    {
        internal enum ButtonType : byte
        {
            Normal,
        }

        [SerializeField] private SpriteSettings normal;

        internal SpriteSettings Get(ButtonType type) => type switch
        {
            ButtonType.Normal => normal,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
