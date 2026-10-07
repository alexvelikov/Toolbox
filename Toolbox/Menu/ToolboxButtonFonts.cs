using System;
using System.Collections.Generic;
using System.Text;
using Containers.src;
using c = System.ConsoleColor;

namespace Toolbox.Menu
{
    public class ToolboxButtonFonts
    {
        private static Font _defaultUnactiveButtonFont = new Font(c.DarkGray, c.Black);

        public static ButtonStatesFonts Scale = new()
        {
            Default = new Font(f: c.White, b: c.Black),
            Selected = new Font(f: c.Black, b: c.White),
            Unactive = _defaultUnactiveButtonFont
        };

        public static ButtonStatesFonts Exit = new()
        {
            Default = new Font(f: c.Red, b: c.Black),
            Selected = new Font(f: c.Black, b: c.DarkRed),
            Unactive = _defaultUnactiveButtonFont
        };

        public static ButtonStatesFonts ToolboxOptionsBorder = new()
        {
            Default = new Font(f: c.Cyan, b: c.Black),
            Selected = new Font(f: c.Yellow, b: c.Yellow),
            Unactive = _defaultUnactiveButtonFont
        };

        public static ButtonStatesFonts ToolboxOptionsLabel = new()
        {
            Default = new Font(f: c.White, b: c.Black),
            Selected = new Font(f: c.Black, b: c.Yellow),
            Unactive = _defaultUnactiveButtonFont
        };
    }
}
