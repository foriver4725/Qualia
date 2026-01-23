namespace MyScripts.Common.Button;

internal interface IColorSettings
{
    Color TextNormal { get; }
    Color TextHovered { get; }
    Color TextClicked { get; }
    Color BackgroundNormal { get; }
    Color BackgroundHovered { get; }
    Color BackgroundClicked { get; }
}

[Serializable]
internal struct ColorSettings : IColorSettings
{
    [SerializeField, ColorUsage(showAlpha: false)] private Color textNormal;
    [SerializeField, ColorUsage(showAlpha: false)] private Color textHovered;
    [SerializeField, ColorUsage(showAlpha: false)] private Color textClicked;
    [SerializeField, ColorUsage(showAlpha: false)] private Color backgroundNormal;
    [SerializeField, ColorUsage(showAlpha: false)] private Color backgroundHovered;
    [SerializeField, ColorUsage(showAlpha: false)] private Color backgroundClicked;

    // aは1に固定して返す
    public readonly Color TextNormal => new(textNormal.r, textNormal.g, textNormal.b);
    public readonly Color TextHovered => new(textHovered.r, textHovered.g, textHovered.b);
    public readonly Color TextClicked => new(textClicked.r, textClicked.g, textClicked.b);
    public readonly Color BackgroundNormal => new(backgroundNormal.r, backgroundNormal.g, backgroundNormal.b);
    public readonly Color BackgroundHovered => new(backgroundHovered.r, backgroundHovered.g, backgroundHovered.b);
    public readonly Color BackgroundClicked => new(backgroundClicked.r, backgroundClicked.g, backgroundClicked.b);
}
