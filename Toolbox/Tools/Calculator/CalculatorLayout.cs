using System;
using System.Collections.Generic;
using System.Text;
using Containers.src;

namespace Toolbox.Calculator
{
    public class CalculatorLayout : ButtonsLayout
    {
        public override void Setup()
        {
            LastRow = 1;
            LastCol = 0;
            
            ButtonsArray = new Button[3][]
            {
                new Button[2]
                {
                    new Button() { ID = ButtonsIDs.Scale },
                    new Button() { ID = ButtonsIDs.Exit },
                },
                new Button[1]
                {
                    new Button() { ID = (int)ButtonsIDs.NewCalculation, Selected = true },
                },
                new Button[1]
                {
                    new Button() { ID = ButtonsIDs.Back }
                }
            };
        }

        public static class ButtonsIDs
        {
            public const int EmptySocket = 0;
            public const int Back = 1;
            public const int NewCalculation = 2;
            public const int Scale = 3;
            public const int Exit = 4;
        }
    }
}
