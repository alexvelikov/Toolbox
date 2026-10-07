using System;
using System.Collections.Generic;
using System.Text;
using Containers.src;
using c = System.ConsoleColor;

namespace Toolbox.Calculator
{
    public class CalculatorButtonFonts
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

        public static ButtonStatesFonts NewCalcButtonBorders = new()
        {
            Default = new Font(f: c.Cyan, b: c.Black),
            Selected = new Font(f: c.Yellow, b: c.Yellow),
            Unactive = _defaultUnactiveButtonFont
        };

        public static ButtonStatesFonts NewCalcButtonLabel = new()
        {
            Default = new Font(f: c.White, b: c.Black),
            Selected = new Font(f: c.Black, b: c.Yellow),
            Unactive = _defaultUnactiveButtonFont
        };

        public static ButtonStatesFonts BackButtonBorders = new()
        {
            Default = new Font(f: c.DarkRed, b: c.Black),
            Selected = new Font(f: c.Red, b: c.Red),
            Unactive = _defaultUnactiveButtonFont
        };

        public static ButtonStatesFonts BackButtonLabel = new()
        {
            Default = new Font(f: c.Red, b: c.Black),
            Selected = new Font(f: c.Black, b: c.Red),
            Unactive = _defaultUnactiveButtonFont
        };
    }
}
