using System;
using System.Collections.Generic;
using System.Text;
using Containers.src;
using Toolbox.Menu;

namespace Toolbox.Calculator
{
    public class CalculatorController
    {
        private static CalculatorLayout _layout = new CalculatorLayout();
        private static bool _isRunning;
        public static void Initialize()
        {
            _isRunning = true;
            CalculatorUI.Initialize(_layout);
            Program.EventChannel.Writer.WriteAsync(CalculatorUI.Render);
        }

        public static void Launch()
        {
            while (_isRunning && !Program.cts.Token.IsCancellationRequested)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                _layout.ProcessInput(keyInfo);
                if(_layout.ActionIdAwaiting == -1)
                {
                    Program.EventChannel.Writer.WriteAsync(() => {
                        CalculatorUI.FillContainers();
                        CalculatorUI.Render();
                    });
                }
                else
                {
                    ExecuteAction(_layout.ActionIdAwaiting);
                }
            }
        }

        private static void ExecuteAction(int actionId)
        {
            switch (actionId)
            {
                case CalculatorLayout.ButtonsIDs.Scale:
                    if (!Window.FullScreen)
                    {
                        Window.SetLargestSizes();
                        Window.fullScreen = true;
                    }
                    else
                    {
                        Window.SetDefaultSizes();
                        Window.fullScreen = false;
                    }
                    break;
                case CalculatorLayout.ButtonsIDs.Exit:
                    Program.cts.Cancel();
                    break;
                case CalculatorLayout.ButtonsIDs.NewCalculation:
                    CalculatorUI.MoveCursorToCommandLine();
                    CalculatorUI.CursorOnCommandLine = true;
                    string expression = Console.ReadLine();
                    CalculatorUI.Expression = expression;
                    CalculatorUI.CalculationResult = Calculator.Calculate(expression);
                    Program.EventChannel.Writer.WriteAsync(() => {
                        CalculatorUI.FillContainers();
                        CalculatorUI.Render();
                    });
                    break;
                case CalculatorLayout.ButtonsIDs.Back:
                    _isRunning = false;
                    break;
                default:
                    // Handle other actions here
                    break;
            }

            _layout.ResetAwaitingResponse(); // Reset the action awaiting after execution
        }
    }
}