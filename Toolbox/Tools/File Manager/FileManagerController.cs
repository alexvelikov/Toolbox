using System;
using System.Collections.Generic;
using System.Text;
using Containers.src;
using Toolbox.Menu;

namespace Toolbox.FileManager
{
    public class FileManagerController
    {
        private static FileManagerLayout _layout = new FileManagerLayout();
        private static bool _isRunning;
        public static void Initialize()
        {
            _isRunning = true;
            FileManagerUI.Initialize(_layout);
            Program.EventChannel.Writer.WriteAsync(FileManagerUI.Render);
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
                        FileManagerUI.FillContainers();
                        FileManagerUI.Render();
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
                case FileManagerLayout.ButtonsIDs.Scale:
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
                case FileManagerLayout.ButtonsIDs.Exit:
                    Program.cts.Cancel();
                    break;
                case FileManagerLayout.ButtonsIDs.Back:
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