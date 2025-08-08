using System.Drawing;

namespace Soft.Geometry.UI.FancyFe
{
    public static class BrandTheme
    {
        // Base colors - pure white surfaces for clarity
        public static Color Background => Color.White;
        public static Color Surface => Color.White;
        public static Color Border => Color.FromArgb(220, 220, 220);
        public static Color Text => Color.FromArgb(28, 28, 28);

        // Neon brand accents - Y2K aesthetic
        public static Color Primary => Color.FromArgb(255, 0, 128);      // hot pink
        public static Color PrimaryBright => Color.FromArgb(255, 64, 164);
        public static Color NeonCyan => Color.FromArgb(0, 190, 255);
        public static Color Purple => Color.FromArgb(106, 90, 205);
        public static Color Yellow => Color.FromArgb(255, 214, 0);
        public static Color Green => Color.FromArgb(0, 153, 0);
        public static Color Blue => Color.FromArgb(0, 120, 215);

        // Shadow Code specific colors
        public static Color ShadowMode => Color.FromArgb(64, 64, 64);
        public static Color ShadowAccent => Color.FromArgb(128, 0, 255);
        public static Color SandboxIndicator => Color.FromArgb(255, 128, 0);

        // Grid and layout
        public static Color GridLines => Color.FromArgb(240, 240, 240);
        public static Color SelectionHighlight => Color.FromArgb(255, 255, 0, 64);
        public static Color ConnectionLine => Color.FromArgb(128, 128, 128);
    }
}
