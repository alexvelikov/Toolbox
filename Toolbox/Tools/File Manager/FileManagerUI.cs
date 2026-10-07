using Containers.src;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Toolbox.Menu;
using static System.Net.Mime.MediaTypeNames;

namespace Toolbox.FileManager
{
    public class FileManagerUI
    {
        public static Program.Views CurrentView = Program.Views.Toolbox;

        private static Container _topBar = new Container();
        private static Container _workbench = new Container();
        private static Container _commandLine = new Container();
        private static Container _buttons = new();
        private static Container _resultBox = new();

        private static FileManagerLayout _layout;

        private static List<string> _note;
        private static int _noteHeight = 0;
        public static string CalculationResult = string.Empty;
        private static string _calculationError
        {
            get
            {
                switch (CalculationResult)
                {
                    case "ERROR number 1":
                        return "[ERROR] Unrecognized characters";
                    case "ERROR number 2":
                        return "[ERROR] Incorrect character order";
                    case "ERROR number 3":
                        return "[ERROR] Division by null";
                    case "ERROR number 4":
                        return "[ERROR] Empty expression";
                    default:
                        return string.Empty;
                }
            }
        }
        public static string Expression = string.Empty;

        public static bool CursorOnCommandLine = false;

        private static UIFonts<DefaultUIElements> _calculatorUIFonts = Program.DefaultUIFonts;


        public static void Initialize(FileManagerLayout layout)
        {
            int windowWidth = 90;
            Window.SetCustomSizes(windowWidth, 19 + _noteHeight);
            Rescale();
            _layout = layout;
            _layout.Setup();
            FillContainers();
            Console.Clear();
        }

        public static void Rescale()
        {
            _topBar.Width = Console.WindowWidth;
            _topBar.Height = 3;
            _topBar.Content = _topBar.DefaultContent;

            _commandLine.Width = Console.WindowWidth;
            _commandLine.Height = 3;
            _commandLine.Content = _commandLine.DefaultContent;

            _workbench.Width = Console.WindowWidth;
            _workbench.Height = Console.WindowHeight - _topBar.Height - _commandLine.Height;
            _workbench.Content = _workbench.DefaultContent;

            _buttons.Width = 25;
            _buttons.Height = _workbench.Height;
            _buttons.Content = _buttons.DefaultContent;

            _resultBox.Width = _workbench.Width - _buttons.Width;
            _resultBox.Height = _workbench.Height;
            _resultBox.Content = _resultBox.DefaultContent;
        }

        public static void MoveCursorToCommandLine()
        {
            Console.CursorLeft = 10;
            Console.CursorTop = _topBar.Height + _workbench.Height + 1;
        }

        public static void FillContainers()
        {
            FillWindowButtons("Calculator");
        }

        public static void Render()
        {
            Console.SetCursorPosition(0, 0);
            int i = 0;
            for (i = 0; i < _topBar.Height; i++)
            {
                _topBar.PrintLine(i);
            }
        }

        private static void FillWindowButtons(string currentModule)
        {
            var heavy = Characters.BoxStyle.Heavy;
            string heavyLineColorCode = _calculatorUIFonts.ReturnFontCodeColorFromElement(DefaultUIElements.HeavyLine);
            string headerLineColorCode = _calculatorUIFonts.ReturnFontCodeColorFromElement(DefaultUIElements.Header);

            string firstRow = heavyLineColorCode;
            firstRow += new string(heavy.Horizontal, _topBar.Width - 12) + heavy.TeeDown;
            firstRow += new string(heavy.Horizontal, 5);
            firstRow += heavy.TeeDown;
            firstRow += new string(heavy.Horizontal, 5);
            _topBar.AlterContent(0, firstRow);

            currentModule = "  " + currentModule.ToUpper();
            int moduleTitleWidth = _topBar.Width - 12;
            currentModule = currentModule.PadRight(moduleTitleWidth);

            string secondRow = $"{headerLineColorCode}{currentModule}";
            currentModule = currentModule.ToUpper();
            string vertical = Characters.BoxStyle.Heavy.Vertical.ToString();
            secondRow += heavyLineColorCode + vertical;
            string scaleColor = _layout.GetButtonFontCode(buttonID: (byte)FileManagerLayout.ButtonsIDs.Scale, font: FileManagerButtonFonts.Scale);
            secondRow += $"{scaleColor}  {Characters.Scale}  ";
            secondRow += heavyLineColorCode + vertical;
            string exitColor = _layout.GetButtonFontCode(buttonID: (byte)FileManagerLayout.ButtonsIDs.Exit, font: FileManagerButtonFonts.Exit);
            secondRow += $"{exitColor}  {Characters.StatusStyle.Ascii.Error}  ";
            _topBar.AlterContent(1, secondRow);

            string thirdRow = $"{heavyLineColorCode}{new string(heavy.Horizontal, _topBar.Width - 12)}{heavy.TeeUp}";
            thirdRow += new string(heavy.Horizontal, 5);
            thirdRow += heavy.TeeUp;
            thirdRow += new string(heavy.Horizontal, 5);
            _topBar.AlterContent(2, thirdRow);
        }
    }
}