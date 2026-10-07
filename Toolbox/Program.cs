using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;
using Toolbox.Menu;
using Toolbox.FileManager;
using Toolbox.Calculator;
using Containers.src;

namespace Toolbox
{
    public class Program
    {
        public static CancellationTokenSource cts = new();
        public static Channel<Action> EventChannel = Channel.CreateUnbounded<Action>();
        public enum Views
        {
            Toolbox, FileManager, Email, Notes,
            Calendar, Calculator, Converter, Clock,
            Snake, TextEditor
        }
        public static Views CurrentView = Views.Toolbox;

        public static readonly Dictionary<Views, Action> viewsDictionaryLoader = new()
        {
            { Views.Toolbox, () => {
                ToolboxUI.Rescale();
                ToolboxUI.FillContainers();
                ToolboxUI.Render();
                }},
            { Views.FileManager, () => { } },
            { Views.Email, () => { } },
            { Views.Notes, () => { } },
            { Views.Calendar, () => { } }, 
            { Views.Calculator, () => {
                CalculatorUI.Rescale();
                CalculatorUI.FillContainers();
                CalculatorUI.Render();
            } },
            { Views.Converter, () => { } },
            { Views.Clock, () => { } },
            { Views.Snake, () => { } },
            { Views.TextEditor, () => { } }
        };

        public static UIFonts<DefaultUIElements> DefaultUIFonts = new()
        {
            UIElementsFontsDict = new Dictionary<DefaultUIElements, Font>
            {
                { DefaultUIElements.Text, new Font(f: ConsoleColor.DarkYellow, b: ConsoleColor.Black) },
                { DefaultUIElements.Header, new Font(f: ConsoleColor.Cyan, b: ConsoleColor.Black) },
                { DefaultUIElements.Value, new Font(f: ConsoleColor.Magenta, b: ConsoleColor.Black) },
                { DefaultUIElements.HeavyLine, new Font(f: ConsoleColor.White, b: ConsoleColor.Black) },
                { DefaultUIElements.DoubleLine, new Font(f: ConsoleColor.Gray, b: ConsoleColor.Black) },
                { DefaultUIElements.Line, new Font(f: ConsoleColor.DarkGray, b: ConsoleColor.Black) },
                { DefaultUIElements.Background, new Font(f: ConsoleColor.Black, b: ConsoleColor.Black) }
            }
        };

        public static void Main(string[] args)
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

            Task.Run(ExecuteEvents);
            Task.Run(PushResizeEvents);

            ToolboxController.Initialize();
            ToolboxController.Launch();
        }

        private static async Task ExecuteEvents()
        {
            await foreach (var action in EventChannel.Reader.ReadAllAsync(cts.Token))
            {
                action();
            }
        }

        private static void PushResizeEvents()
        {
            int LastW = Console.WindowWidth;
            int LastH = Console.WindowHeight;
            DateTime lastResize = DateTime.MinValue;
            bool resized = false;
            const int debounceMs = 200;

            while (!cts.Token.IsCancellationRequested)
            {
                int w = Console.WindowWidth;
                int h = Console.WindowHeight;

                if (w != LastW || h != LastH)
                {
                    LastW = w;
                    LastH = h;

                    // Mark the time of the last resize
                    resized = true;
                    lastResize = DateTime.Now;
                }

                // If enough time has passed since last resize, trigger redraw
                if (resized && (DateTime.Now - lastResize).TotalMilliseconds > debounceMs)
                {
                    resized = false;
                    EventChannel.Writer.WriteAsync(viewsDictionaryLoader[CurrentView]);
                }

                Thread.Sleep(50); // Check ~20 times per second
            }
        }
    }
}
