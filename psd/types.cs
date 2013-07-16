
namespace psd
{
    public enum ColorMode
    {
        Unknown,

        Mono,
        GrayScale,
        RGB,
        CMYK,
        Lab,
    }

    public enum BlendMode
    {
        Unknown,

        PassThrough,
        Normal,
        Dissolve,
        Darken,
        Multiply,
        ColorBurn,
        LinearBurn,
        DarkerColor,
        Lighten,
        Screen,
        ColorDodge,
        LinearDodge,
        LighterColor,
        Overlay,
        SoftLight,
        HardLight,
        VividLight,
        LinearLight,
        PinLight,
        HardMix,
        Difference,
        Exclusion,
        Subtract,
        Divide,
        Hue,
        Saturation,
        Color,
        Luminosity,

        Count
    }

    public enum DefaultColor
    {
        Unknown,

        Black,
        White,
    }
}
