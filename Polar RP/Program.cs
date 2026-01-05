using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using ConsoleWriter;
using Polar.Core;
using log4net;
using log4net.Config;
using Polar.Utilities;
using Polar.HabboHotel.Bots;
using System.Threading.Tasks;

namespace Polar
{
    public class Program
    {
        private const int MF_BYCOMMAND = 0x00000000;
        public const int SC_CLOSE = 0xF060;
        private static readonly ILog log = LogManager.GetLogger("Polar.Program");
        /*
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();
         */
        [SecurityPermission(action: SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
       
        public static async Task Main(string[] Args)
        {
            //DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_CLOSE, MF_BYCOMMAND);

            XmlConfigurator.Configure();

            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();
            await InitEnvironment();

            while (PolarEnvironment.IsLive)
            {
                Console.CursorVisible = true;
                ConsoleCommandHandler.InvokeCommand(Console.ReadLine());
            }
        }

        public static async Task InitEnvironment()
        {
            if (PolarEnvironment.IsLive)
                return;

            Console.CursorVisible = false;
            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += MyHandler;
            OrionScreen();
            await PolarEnvironment.Initialize();
        }

        private static void OrionScreen()
        {
            Console.Clear();
            Console.WindowHeight = 33;
            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine(@" POLAR SERVER v" + PolarEnvironment.PrettyBuild);
            Console.WriteLine(@" Emulador exclusivo para comunidades virtuales basadas en sistema de ROLEPLAY");
            Console.WriteLine(@" ");
            Console.WriteLine(@" Codificado por: Hefesto y JuanGomez");
            Console.WriteLine(@" AÑO: 2017 - 2024");
            Console.WriteLine();

            Console.WriteLine(
                Console.LargestWindowWidth > 20
                ? @"-----------------------------------------------------------------------------------------------------------------------"
                : @"");


            Console.ForegroundColor = ConsoleColor.Green;
        }

        private static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Logging.DisablePrimaryWriting(true);
            var e = (Exception)args.ExceptionObject;
            Logging.LogCriticalException("SYSTEM CRITICAL EXCEPTION: " + e);
            //PolarEnvironment.PerformShutDown(true);
        }

        private enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private delegate bool EventHandler(CtrlType sig);
    }
}