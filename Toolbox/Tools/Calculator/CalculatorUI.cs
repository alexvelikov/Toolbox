using Containers.src;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Toolbox.Menu;
using static System.Net.Mime.MediaTypeNames;

namespace Toolbox.Calculator
{
    public class CalculatorUI
    {
        public static Program.Views CurrentView = Program.Views.Toolbox;

        private static Container _topBar = new Container();
        private static Container _workbench = new Container();
        private static Container _commandLine = new Container();
        private static Container _buttons = new();
        private static Container _resultBox = new();

        private static CalculatorLayout _layout;

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


        public static void Initialize(CalculatorLayout layout)
        {
            int windowWidth = 90;
            ConfigureNote(windowWidth);
            Window.SetCustomSizes(windowWidth, 19 + _noteHeight);
            Rescale();
            _layout = layout;
            _layout.Setup();
            FillContainers();
            Console.Clear();
        }

        private static void ConfigureNote(int windowWidth)
        {
            _note = new();
            int noteHeight = 0;
            string[] note =
                {
                    "<DarkYellow> The calculator can solve complex expressions and uses the following operations: ",
                    "<Magenta>+", "<DarkYellow>, ", "<Magenta>-", "<DarkYellow>, ", "<Magenta>*", "<DarkYellow>, ", "<Magenta>/",
                     "<DarkYellow>, ", "<Magenta>^", "<DarkYellow>, ", "<Magenta>(", "<DarkYellow>, ", "<Magenta>)",
                    "<DarkYellow>. The calculator can handle non-integer values and handles incorrect expressions. "
                };

            int lineWidth = windowWidth - 4;
            int idx = 0;
            _note.Add(string.Empty);
            foreach (string noteString in note)
            {
                string textOnly = ColorsManager.ReturnTextOnly(noteString);
                string colorCode = noteString.Substring(0, noteString.IndexOf(textOnly));
                Func<int> textLength = () => textOnly.Length;

                while (textLength() > 0)
                {
                    if (textLength() - lineWidth < 0) // fits in line
                    {
                        lineWidth -= textLength();
                        _note[idx] += colorCode + textOnly;
                        textOnly = string.Empty;
                    }
                    else // does not fit in line
                    {
                        string text = textOnly.Substring(0, lineWidth);
                        _note[idx] += colorCode + text;
                        textOnly = textOnly.Substring(lineWidth);
                        lineWidth = windowWidth - 4;
                        _note.Add(string.Empty);
                        idx++;
                        noteHeight++;
                    }
                }
            }

            _noteHeight = noteHeight;
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
            FillWorkbench();
            FillCommandLine();
        }

        public static void Render()
        {
            Console.SetCursorPosition(0, 0);
            int i = 0;
            for (i = 0; i < _topBar.Height; i++)
            {
                _topBar.PrintLine(i);
            }

            for (i = 0; i < _workbench.Height; i++)
            {
                _workbench.PrintLine(i);
            }

            for (i = 0; i < _commandLine.Height - 1; i++)
            {
                _commandLine.PrintLine(i);
            }
            _commandLine.Print(i);

            if(CursorOnCommandLine)
            {
                MoveCursorToCommandLine();
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
            string scaleColor = _layout.GetButtonFontCode(buttonID: (byte)CalculatorLayout.ButtonsIDs.Scale, font: Menu.ToolboxButtonFonts.Scale);
            secondRow += $"{scaleColor}  {Characters.Scale}  ";
            secondRow += heavyLineColorCode + vertical;
            string exitColor = _layout.GetButtonFontCode(buttonID: (byte)CalculatorLayout.ButtonsIDs.Exit, font: Menu.ToolboxButtonFonts.Exit);
            secondRow += $"{exitColor}  {Characters.StatusStyle.Ascii.Error}  ";
            _topBar.AlterContent(1, secondRow);

            string thirdRow = $"{heavyLineColorCode}{new string(heavy.Horizontal, _topBar.Width - 12)}{heavy.TeeUp}";
            thirdRow += new string(heavy.Horizontal, 5);
            thirdRow += heavy.TeeUp;
            thirdRow += new string(heavy.Horizontal, 5);
            _topBar.AlterContent(2, thirdRow);
        }

        private static void FillWorkbench()
        {
            GetWorkbenchNote();
            GetButtons();
            GetResultBox();

            int noteBoxHeight = _note.Count + 3;
            for (int i = noteBoxHeight; i < _workbench.Height; i++)
            {
                _workbench.Content[i] = _buttons.Content[i];
            }
            for (int i = noteBoxHeight; i < _workbench.Height; i++)
            {
                _workbench.Content[i].AddRange(_resultBox.Content[i]);
            }
        }

        private static void GetWorkbenchNote()
        {
            var horizonalLine = Characters.BoxStyle.Single.Horizontal;
            string backgroundColorCode = _calculatorUIFonts.ReturnFontCodeColorFromElement(DefaultUIElements.Background);
            string lineColorCode = _calculatorUIFonts.ReturnFontCodeColorFromElement(DefaultUIElements.Line);

            _workbench.AlterContent(0, backgroundColorCode + new string(' ', _workbench.Width));
            for(int i = 1; i <= _note.Count; i++)
            {
                _workbench.AlterContent(i, backgroundColorCode + $"  {_note[i - 1]}");
            }
            _workbench.AlterContent(_note.Count + 1, backgroundColorCode + new string(' ', _workbench.Width));
            _workbench.AlterContent(_note.Count + 2, lineColorCode + new string(horizonalLine, _workbench.Width));
        }

        private static void GetButtons()
        {
            const string newCalculation = "New Calculation";
            const string back = "Go Back";
            byte newCalcButtonID = CalculatorLayout.ButtonsIDs.NewCalculation;
            byte backButtonID = CalculatorLayout.ButtonsIDs.Back;
            string newCalcBorderColor = _layout.GetButtonFontCode(newCalcButtonID, CalculatorButtonFonts.NewCalcButtonBorders);
            string newCalcTextColor = _layout.GetButtonFontCode(newCalcButtonID, CalculatorButtonFonts.NewCalcButtonLabel);
            string backBorderColor = _layout.GetButtonFontCode(backButtonID, CalculatorButtonFonts.BackButtonBorders);
            string backTextColor = _layout.GetButtonFontCode(backButtonID, CalculatorButtonFonts.BackButtonLabel);

            var doubleChars = Characters.BoxStyle.Double;
            var heavyChars = Characters.BoxStyle.Heavy;
            string horizontalLine = new string(doubleChars.Horizontal, newCalculation.Length + 4);

            int row = 0;
            for(; row < _noteHeight + 3; row++)
            {
                _buttons.AlterContent(row, "<Default>" + new string(' ', _buttons.Width));
            }

            row++;
            _buttons.AlterContent(row++, new string(' ', _buttons.Width));

            string text;
            text = $"<White>   {newCalcBorderColor}{doubleChars.TopLeft}{horizontalLine}{doubleChars.TopRight}";
            _buttons.AlterContent(row++, text);

            text = $"<White>   {newCalcBorderColor}{doubleChars.Vertical}{newCalcTextColor}  {newCalculation.ToUpper()}  {newCalcBorderColor}{doubleChars.Vertical}";
            _buttons.AlterContent(row++, text);

            text = $"<White>   {newCalcBorderColor}{doubleChars.BottomLeft}{horizontalLine}{doubleChars.BottomRight}";
            _buttons.AlterContent(row++, text);

            _buttons.AlterContent(row++, new string(' ', _buttons.Width));

            text = $"<White>   {backBorderColor}{doubleChars.TopLeft}{horizontalLine}{doubleChars.TopRight}";
            _buttons.AlterContent(row++, text);

            int totalWidth = newCalculation.Length + 2;
            int leftPadding = totalWidth / 2;
            text = $"<White>   {backBorderColor}{doubleChars.Vertical}{backTextColor} {back.ToUpper().PadLeft(leftPadding).PadRight(totalWidth)} {backBorderColor}{doubleChars.Vertical}";
            _buttons.AlterContent(row++, text);

            text = $"<White>   {backBorderColor}{doubleChars.BottomLeft}{horizontalLine}{doubleChars.BottomRight}";
            _buttons.AlterContent(row++, text);

            for (; row < _buttons.Height; row++)
            {
                _buttons.AlterContent(row, "<Default>" + new string(' ', _buttons.Width));
            }
        }

        private static void GetResultBox()
        {
            string backgroundColorCode = _calculatorUIFonts.ReturnFontCodeColorFromElement(DefaultUIElements.Background);
            string textColorCode = _calculatorUIFonts.ReturnFontCodeColorFromElement(DefaultUIElements.Text);
            string lineColorCode = _calculatorUIFonts.ReturnFontCodeColorFromElement(DefaultUIElements.Line);
            string valueColorCode = _calculatorUIFonts.ReturnFontCodeColorFromElement(DefaultUIElements.Value);

            int row = 0;
            for (; row < _noteHeight + 3; row++)
            {
                _buttons.AlterContent(row, "<Default>" + new string(' ', _buttons.Width));
            }
            string result = _calculationError == string.Empty ? CalculationResult : _calculationError;

            row += 3;
            var singleChars = Characters.BoxStyle.Single;
            string text = $"{lineColorCode}    {singleChars.TopLeft}";
            text += new string(singleChars.Horizontal, _resultBox.Width * 3 / 4);
            text += singleChars.TopRight;
            _resultBox.AlterContent(row++, text);

            string padding = new string(' ', _resultBox.Width * 3/4 - 13 - Expression.Length);
            text = $"{lineColorCode}    {singleChars.Vertical}";
            text += $" {textColorCode}Expression: {valueColorCode}{Expression}";
            text += padding;
            text += $"{lineColorCode}" + singleChars.Vertical;
            _resultBox.AlterContent(row++, text);

            text = $"{lineColorCode}    {singleChars.Vertical}";
            text += new string(singleChars.Horizontal, _resultBox.Width * 3/4);
            text += singleChars.Vertical;
            _resultBox.AlterContent(row++, text);

            padding = new string(' ', _resultBox.Width * 3/4 - 9 - result.Length);
            text = $"{lineColorCode}    {singleChars.Vertical}";
            text += $" {textColorCode}Result: {valueColorCode}{result}";
            text += padding;
            text += $"{lineColorCode}" + singleChars.Vertical;
            _resultBox.AlterContent(row++, text);

            text = $"{lineColorCode}    {singleChars.BottomLeft}";
            text += new string(singleChars.Horizontal, _resultBox.Width * 3/4);
            text += singleChars.BottomRight;
            _resultBox.AlterContent(row++, text);

            for(; row < _resultBox.Height; row++)
            {
                _resultBox.AlterContent(row, backgroundColorCode + new string(' ', _resultBox.Width));
            }
        }

        private static void FillCommandLine()
        {
            string textColorCode = _calculatorUIFonts.ReturnFontCodeColorFromElement(DefaultUIElements.Text);
            string heavyLineColorCode = _calculatorUIFonts.ReturnFontCodeColorFromElement(DefaultUIElements.HeavyLine);

            var heavy = Characters.BoxStyle.Heavy;
            string horizontalLine = new string(heavy.Horizontal, _commandLine.Width);
            _commandLine.AlterContent(0, $"{heavyLineColorCode}{horizontalLine}");
            string text = $"<Yellow>{Characters.ArrowStyle.Unicode.Right}{textColorCode}";
            _commandLine.AlterContent(1, $" {text} Enter:");
            _commandLine.AlterContent(2, $"{heavyLineColorCode}{horizontalLine}");
        }
    }
}