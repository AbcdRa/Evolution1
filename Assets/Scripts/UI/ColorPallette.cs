


using UnityEngine;

internal class ColorPallette
{
    public static Color green = FromArgb(155, 197, 61);
    public static Color dark = FromArgb(4, 42, 43);
    public static Color neon = FromArgb(164, 145, 211);
    public static Color orange = FromArgb(255, 164, 0);
    public static Color red = FromArgb(254, 95, 0);
    public static Color cream = FromArgb(245, 255, 178);

    public static Color FromArgb(int r, int g, int b)
    {
        return new Color(r/255f, g/255f, b/255f);
    }
}

