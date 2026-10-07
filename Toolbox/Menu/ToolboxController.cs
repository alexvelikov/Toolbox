using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.FileManager;
using Toolbox.Calculator;
using Containers.src;

namespace Toolbox.Menu
{
    public class ToolboxController
    {
        private static ToolboxLayout _layout = new ToolboxLayout();

        public static void Initialize()
        {
            Program.CurrentView = Program.Views.Toolbox;
            ToolboxUI.Initialize(_layout);
            ToolboxUI.Render();
        }

        public static void Launch()
        {
            while (!Program.cts.Token.IsCancellationRequested)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                _layout.ProcessInput(keyInfo);
                if(_layout.ActionIdAwaiting == -1)
                {
                    Program.EventChannel.Writer.WriteAsync(() => {
                        ToolboxUI.FillContainers();
                        ToolboxUI.Render();
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
                case ToolboxLayout.ButtonsIDs.Scale:
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
                case ToolboxLayout.ButtonsIDs.Exit:
                    Program.cts.Cancel();
                    break;
                case (int)ToolboxLayout.ButtonsIDs.ToolboxOptions.FileManager:
                    Program.CurrentView = Program.Views.FileManager;
                    FileManagerController.Initialize();
                    FileManagerController.Launch();
                    Initialize(); // Re-initialize the toolbox layout and UI
                    break;
                case (int)ToolboxLayout.ButtonsIDs.ToolboxOptions.Calendar:
                    Program.CurrentView = Program.Views.Calendar;
                    //CalendarController.Initialize();
                    //CalendarController.Launch();
                    break;
                case (int)ToolboxLayout.ButtonsIDs.ToolboxOptions.Email:
                    Program.CurrentView = Program.Views.Email;
                    //EmailsController.Initialize();
                    //EmailsController.Launch();
                    break;
                case (int)ToolboxLayout.ButtonsIDs.ToolboxOptions.Converter:
                    Program.CurrentView = Program.Views.Converter;
                    //ConverterController.Initialize();
                    //ConverterController.Launch();
                    break;
                case (int)ToolboxLayout.ButtonsIDs.ToolboxOptions.TextEditor:
                    Program.CurrentView = Program.Views.TextEditor;
                    //TextEditorController.Initialize();
                    //TextEditorController.Launch();
                    break;
                case (int)ToolboxLayout.ButtonsIDs.ToolboxOptions.Clock:
                    Program.CurrentView = Program.Views.Clock;
                    //ClockController.Initialize();
                    //ClockController.Launch();
                    break;
                case (int)ToolboxLayout.ButtonsIDs.ToolboxOptions.Snake:
                    Program.CurrentView = Program.Views.Snake;
                    //SnakeController.Initialize();
                    //SnakeController.Launch();
                    break;
                case (int)ToolboxLayout.ButtonsIDs.ToolboxOptions.Calculator:
                    Program.CurrentView = Program.Views.Calculator;
                    CalculatorController.Initialize();
                    CalculatorController.Launch();
                    Initialize(); // Re-initialize the toolbox layout and UI
                    break;
                case (int)ToolboxLayout.ButtonsIDs.ToolboxOptions.Notes:
                    Program.CurrentView = Program.Views.Notes;
                    //NotesController.Initialize();
                    //NotesController.Launch();
                    break;
                default:
                    // Handle other actions here
                    break;
            }

            _layout.ResetAwaitingResponse(); // Reset the action awaiting after execution
        }
    }
}