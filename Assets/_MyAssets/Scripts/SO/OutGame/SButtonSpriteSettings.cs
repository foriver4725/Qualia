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

            Pause_Resume,
            Pause_BackToTitle,
            Pause_BackToDesktop,
        }

        [SerializeField] private SpriteSettings normal;
        [SerializeField] private SpriteSettings saveSlot;
        [Space(10)]
        [SerializeField] private SpriteSettings pause_Resume;
        [SerializeField] private SpriteSettings pause_BackToTitle;
        [SerializeField] private SpriteSettings pause_BackToDesktop;

        internal SpriteSettings Get(ButtonType type) => type switch
        {
            ButtonType.Normal => normal,
            ButtonType.SaveSlot => saveSlot,
            ButtonType.Pause_Resume => pause_Resume,
            ButtonType.Pause_BackToTitle => pause_BackToTitle,
            ButtonType.Pause_BackToDesktop => pause_BackToDesktop,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
