using System.Drawing;

namespace Soft.Geometry.UI
{
    // Centralized brand palette for a white UI with bold brand accents.
    public static class BrandTheme
    {
        // UI surface colors
        public static Color Background => Color.White;
        public static Color Surface => Color.White;
        public static Color Border => Color.FromArgb(230, 230, 230); // #E6E6E6
        public static Color Text => Color.FromArgb(28, 28, 28);        // ~#1C1C1C

        // Brand accents (update hex if you have exact values)
        public static Color Primary => Color.FromArgb(255, 76, 142);       // pink
        public static Color PrimaryDark => Color.FromArgb(217, 60, 122);   // deeper pink
        public static Color Purple => Color.FromArgb(106, 74, 203);        // purple
        public static Color Yellow => Color.FromArgb(255, 193, 7);         // yellow
        public static Color Green => Color.FromArgb(67, 160, 71);          // green
        public static Color Blue => Color.FromArgb(30, 136, 229);          // blue

        // Optional tonal variants
        public static Color SurfaceShadow => Color.FromArgb(240, 240, 240);
    }
}
