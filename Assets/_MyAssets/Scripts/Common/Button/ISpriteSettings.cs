namespace MyScripts.Common.Button;

internal interface ISpriteSettings
{
    Sprite SpriteDefault { get; }
    Sprite SpriteHovered { get; }
    Sprite SpriteClicked { get; }
}

[Serializable]
internal sealed class SpriteSettings : ISpriteSettings
{
    [SerializeField] private Sprite spriteDefault;
    [SerializeField] private Sprite spriteHovered;
    [SerializeField] private Sprite spriteClicked;

    public Sprite SpriteDefault => spriteDefault;
    public Sprite SpriteHovered => spriteHovered;
    public Sprite SpriteClicked => spriteClicked;
}
