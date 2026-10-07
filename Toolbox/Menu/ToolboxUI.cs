using Containers.src;
using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Calculator;
using static System.Net.Mime.MediaTypeNames;

namespace Toolbox.Menu
{
    public class ToolboxUI
    {
        public static Program.Views CurrentView = Program.Views.Toolbox;

        private static Container _topBar = new Container();
        private static Container _contentBorder = new Container();
        private static Container _content = new Container();

        private static ToolboxLayout _layout;

        private class ToolboxUIMetrics
        {
            public static int StringLength = 40;
            public static int ButtonPaddingW = 3;
            public static int ButtonBorderW = 1;
            public static int ButtonMarginW = 5;
            public static int ButtonPaddingH = 0;
            public static int ButtonBorderH = 1;
            public static int ButtonMarginH = 1;

            public static Func<string, int> GetButtonWidth = (string text) => text.Length + ((ButtonPaddingW + ButtonBorderW) * 2);
            public static int GridWidth => StringLength + ((ButtonPaddingW + ButtonBorderW) * 2 *GridColumns) + (ButtonMarginW * (GridColumns - 1));
            public static Func<int, int> GetLength = (int length) => length + ((ButtonPaddingW + ButtonBorderW) * GridColumns) + (ButtonMarginW * (GridColumns - 1));
            public static int GridHeight => GridRows * (1 + (ButtonPaddingH + ButtonBorderH) * 2) + (ButtonMarginH * (GridRows - 1));

            public static int GridRows = 3;
            public static int GridColumns = 3;
            public static int GridMarginW = 6;
            public static int GridMarginH = 3;

            public static int WindowWidth => GridWidth + (GridMarginW * 2);
            public static int WindowHeight => GridHeight + (GridMarginH * 2);
        }

        private static string[] _elementsNames =
        {
            "File Manager", "Calendar", "Email",
            "Converter", "Text Editor", "Clock",
            "Snake Game", "Calculator", "Notes",
        };

        private static UIFonts<DefaultUIElements> _toolboxUIFonts = Program.DefaultUIFonts;


        public static void Initialize(ToolboxLayout layout)
        {
            _layout = layout;
            int height = ToolboxUIMetrics.WindowHeight + 3; // +3 for top bar
            Window.SetCustomSizes(ToolboxUIMetrics.WindowWidth, height);
            Rescale();
            _layout.Setup();
            FillContainers();
            Console.Clear();
        }

        public static void Rescale()
        {
            _topBar.Width = Console.WindowWidth;
            _topBar.Height = 3;
            _topBar.Content = _topBar.DefaultContent;

            _contentBorder.Width = Console.WindowWidth;
            _contentBorder.Height = Console.WindowHeight;
            _contentBorder.Content = _contentBorder.DefaultContent;

            _content.Width = Console.WindowWidth - 1;
            _content.Height = Console.WindowHeight - 1;
            _content.Content = _content.DefaultContent;
        }

        public static void FillContainers()
        {
            FillWindowButtons("Toolbox");
            FillContentContainer();
            FillContentBorders();
        }

        public static void Render()
        {
            Console.SetCursorPosition(0, 0);
            int i = 0;
            

            for (; i < _topBar.Height; i++)
            {
                _topBar.PrintLine(i);
            }

            for(i = 0; i < _contentBorder.Height - 1; i++)
            {
                _contentBorder.PrintLine(i);
            }
            _contentBorder.Print(i);
        }

        private static void FillWindowButtons(string currentModule)
        {
            var heavy = Characters.BoxStyle.Heavy;
            string heavyLineColorCode = _toolboxUIFonts.ReturnFontCodeColorFromElement(DefaultUIElements.HeavyLine);
            string headerLineColorCode = _toolboxUIFonts.ReturnFontCodeColorFromElement(DefaultUIElements.Header);

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
            string scaleColor = _layout.GetButtonFontCode(buttonID: ToolboxLayout.ButtonsIDs.Scale, font: ToolboxButtonFonts.Scale);
            secondRow += $"{scaleColor}  {Characters.Scale}  ";
            secondRow += heavyLineColorCode + vertical;
            string exitColor = _layout.GetButtonFontCode(buttonID: ToolboxLayout.ButtonsIDs.Exit, font: ToolboxButtonFonts.Exit);
            secondRow += $"{exitColor}  {Characters.StatusStyle.Ascii.Error}  ";
            _topBar.AlterContent(1, secondRow);

            string thirdRow = $"{heavyLineColorCode}{new string(heavy.Horizontal, _topBar.Width - 12)}{heavy.TeeUp}";
            thirdRow += new string(heavy.Horizontal, 5);
            thirdRow += heavy.TeeUp;
            thirdRow += new string(heavy.Horizontal, 5);
            _topBar.AlterContent(2, thirdRow);
        }

        private static void FillContentBorders()
        {
            var singleChars = Characters.BoxStyle.Single;
            string singleLineColorCode = _toolboxUIFonts.ReturnFontCodeColorFromElement(DefaultUIElements.Line);
            int row = 0;

            _contentBorder.AlterContent(row++, $"{singleLineColorCode}{singleChars.TopLeft}{new string(singleChars.Horizontal, _contentBorder.Width - 2)}{singleChars.TopRight}");
            for(; row < _contentBorder.Height - 1; row++)
            {
                _contentBorder.AlterContent(row, $"{singleLineColorCode}{singleChars.Vertical}");
                _contentBorder.AppendContent(row, _content.Content[row - 1]);
                _contentBorder.AppendContent(row, singleLineColorCode + singleChars.Vertical);
            }
            _contentBorder.AlterContent(row, $"{singleLineColorCode}{singleChars.BottomLeft}{new string(singleChars.Horizontal, _contentBorder.Width - 2)}{singleChars.BottomRight}");
        }

        private static void FillContentContainer()
        {
            var singleChars = Characters.BoxStyle.Single;
            string singleColorCode = _toolboxUIFonts.ReturnFontCodeColorFromElement(DefaultUIElements.Line);
            int row = 0;
            int buttonID = (int)ToolboxLayout.ButtonsIDs.ToolboxOptions.FileManager;

            // Row of buttons
            for(int i = 0; i < _elementsNames.Length; i += 3)
            {
                int currentRowWidth = ToolboxUIMetrics.GetLength(_elementsNames[i].Length + _elementsNames[i + 1].Length + _elementsNames[i + 2].Length);
                int rowMargin = ToolboxUIMetrics.WindowWidth;
                rowMargin -= ToolboxUIMetrics.GetButtonWidth(_elementsNames[i]);
                rowMargin -= ToolboxUIMetrics.GetButtonWidth(_elementsNames[i + 1]);
                rowMargin -= ToolboxUIMetrics.GetButtonWidth(_elementsNames[i + 2]);
                rowMargin -= ToolboxUIMetrics.ButtonMarginW * 2;
                rowMargin /= 2;
                string rowMarginString = currentRowWidth < ToolboxUIMetrics.GridWidth ? new string(' ', rowMargin - 2) : "";

                var doubledCharacters = Characters.BoxStyle.Double;

                // buttons top border
                string text = $"<Default>{rowMarginString}"; // grid margin left
                for (int column = 0; column < ToolboxUIMetrics.GridColumns; column++)
                {
                    text += $"{_layout.GetButtonFontCode(buttonID: (byte)buttonID, font: ToolboxButtonFonts.ToolboxOptionsBorder)}{doubledCharacters.TopLeft}"; // Top left corner
                    text += $"{new string(doubledCharacters.Horizontal, ToolboxUIMetrics.GetButtonWidth(_elementsNames[i + column]) - 2)}"; // Top border
                    text += $"{doubledCharacters.TopRight}"; // Top right corner
                    text += column == ToolboxUIMetrics.GridColumns - 1? "<Default>" : $"<Default>{new string(' ', ToolboxUIMetrics.ButtonMarginW)}"; // Margin right
                    buttonID++;
                }
                text += $"{rowMarginString}"; // margin right
                _content.AlterContent(row++, text);
                buttonID -= ToolboxUIMetrics.GridColumns;

                // buttons vertical padding
                for (int j = 0; j < ToolboxUIMetrics.ButtonPaddingH; j++)
                {
                    text = $"<Default>{rowMarginString}"; // margin left
                    for (int column = 0; column < ToolboxUIMetrics.GridColumns; column++)
                    {
                        text += $"{_layout.GetButtonFontCode(buttonID: (byte)(buttonID - ToolboxUIMetrics.GridColumns + column), font: ToolboxButtonFonts.ToolboxOptionsBorder)}{doubledCharacters.Vertical}"; // Left border
                        text += $"{new string(' ', ToolboxUIMetrics.GetButtonWidth(_elementsNames[i + column]) - 2)}"; // Padding
                        text += $"{doubledCharacters.Vertical}"; // Right border
                        text += column == ToolboxUIMetrics.GridColumns - 1 ? "<Default>" : $"<Default>{new string(' ', ToolboxUIMetrics.ButtonMarginW)}"; // Margin right
                        buttonID++;
                    }
                    text += $"{rowMarginString}"; // grind margin right
                    _content.AlterContent(row++, text);
                    buttonID -= ToolboxUIMetrics.GridColumns;
                }

                // buttons label
                text = $"<Default>{rowMarginString}"; // grid margin left
                for (int column = 0; column < ToolboxUIMetrics.GridColumns; column++)
                {
                    text += $"{_layout.GetButtonFontCode(buttonID: (byte)buttonID, font: ToolboxButtonFonts.ToolboxOptionsBorder)}{doubledCharacters.Vertical}"; // left border
                    string buttonContentLabelColor = _layout.GetButtonFontCode(buttonID: (byte)buttonID, font: ToolboxButtonFonts.ToolboxOptionsLabel);
                    string buttonContentBorderColor = _layout.GetButtonFontCode(buttonID: (byte)buttonID, font: ToolboxButtonFonts.ToolboxOptionsBorder);
                    text += $"{buttonContentLabelColor}{new string(' ', ToolboxUIMetrics.ButtonPaddingW)}{_elementsNames[i + column].ToUpper()}{new string(' ', ToolboxUIMetrics.ButtonPaddingW)}"; // content
                    text += $"{buttonContentBorderColor}{doubledCharacters.Vertical}"; // right border
                    buttonID++;
                }
                text += $"{rowMarginString}"; // margin right
                text += singleColorCode + " " + singleChars.Vertical; // right screen border
                _content.AlterContent(row++, text);
                buttonID -= ToolboxUIMetrics.GridColumns;

                // buttons vertical padding
                for (int j = 0; j < ToolboxUIMetrics.ButtonPaddingH; j++)
                {
                    text = $"<Default>{rowMarginString}"; // margin left
                    for (int column = 0; column < ToolboxUIMetrics.GridColumns; column++)
                    {
                        text += $"{_layout.GetButtonFontCode(buttonID: (byte)(buttonID - ToolboxUIMetrics.GridColumns + column), font: ToolboxButtonFonts.ToolboxOptionsBorder)}{doubledCharacters.Vertical}"; // Left border
                        text += $"{new string(' ', ToolboxUIMetrics.GetButtonWidth(_elementsNames[i + column]) - 2)}"; // Padding
                        text += $"{doubledCharacters.Vertical}"; // Right border
                        text += column == ToolboxUIMetrics.GridColumns - 1 ? "<Default>" : $"<Default>{new string(' ', ToolboxUIMetrics.ButtonMarginW)}"; // Margin right                
                        buttonID++;
                    }
                    text += $"{rowMarginString}"; // grind margin right
                    _content.AlterContent(row++, text);
                    buttonID -= ToolboxUIMetrics.GridColumns;
                }

                // Buttons bottom border
                text = $"<Default>{rowMarginString}"; // grid margin left
                for (int column = 0; column < ToolboxUIMetrics.GridColumns; column++)
                {
                    text += $"{_layout.GetButtonFontCode(buttonID: (byte)buttonID, font: ToolboxButtonFonts.ToolboxOptionsBorder)}{doubledCharacters.BottomLeft}"; // Bottom left corner
                    text += $"{new string(doubledCharacters.Horizontal, ToolboxUIMetrics.GetButtonWidth(_elementsNames[i + column]) - 2)}"; // Bottom border
                    text += $"{doubledCharacters.BottomRight}"; // Bottom right corner
                    text += column == ToolboxUIMetrics.GridColumns - 1 ? "<Default>" : $"<Default>{new string(' ', ToolboxUIMetrics.ButtonMarginW)}"; // Margin right             
                    buttonID++;
                }
                text += $"{rowMarginString}"; // grid margin right
                _content.AlterContent(row++, text);

                for (int j = 0; j < ToolboxUIMetrics.ButtonMarginH; j++)
                {
                    _content.AlterContent(row++, new string(' ', _content.Width - 1) + singleColorCode + singleChars.Vertical);
                }
            }
            
        }
    }
}