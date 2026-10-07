using System;
using System.Collections.Generic;
using System.Text;
using Containers.src;

namespace Toolbox.Menu
{
    public class ToolboxLayout : ButtonsLayout
    {
        public override void Setup()
        {
            LastRow = 1;
            LastCol = 0;
            
            ButtonsArray = new Button[4][]
            {
                new Button[3]
                {
                    new Button() { ID = ButtonsIDs.EmptySocket, Unactive = true },
                    new Button() { ID = ButtonsIDs.Scale},
                    new Button() { ID = ButtonsIDs.Exit}
                },
                new Button[3]
                {
                    new Button() { ID = (int)ButtonsIDs.ToolboxOptions.FileManager, Selected = true },
                    new Button() { ID = (int)ButtonsIDs.ToolboxOptions.Calendar},
                    new Button() { ID = (int)ButtonsIDs.ToolboxOptions.Email}
                },
                new Button[3]
                {
                    new Button() { ID = (int)ButtonsIDs.ToolboxOptions.Converter},
                    new Button() { ID = (int)ButtonsIDs.ToolboxOptions.TextEditor},
                    new Button() { ID = (int)ButtonsIDs.ToolboxOptions.Clock}
                },
                new Button[3]
                {
                    new Button() { ID = (int)ButtonsIDs.ToolboxOptions.Snake},
                    new Button() { ID = (int)ButtonsIDs.ToolboxOptions.Calculator},
                    new Button() { ID = (int)ButtonsIDs.ToolboxOptions.Notes}
                }
            };
        }

        public static class ButtonsIDs
        {
            public const int EmptySocket = 0;
            public enum ToolboxOptions
            {
                FileManager = 1, Calendar = 2, Email = 3,
                Converter = 4, TextEditor = 5, Clock = 6,
                Snake = 7, Calculator = 8, Notes = 9
            }

            public const int Scale = 10;
            public const int Exit = 11;
        }
    }
}
