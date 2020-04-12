using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Terrascape.Debugging;
using Console = Colorful.Console;

#nullable enable

namespace Terrascape.Helpers
{
	internal static class ConsoleHelper
	{
		#region DLL Imports
		// ReSharper disable UnusedMember.Local
		
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern uint GetConsoleProcessList(uint[] p_process_list, uint p_process_count);

		[DllImport("kernel32.dll")]
		private static extern IntPtr GetConsoleWindow();

		[DllImport("user32.dll")]
		private static extern bool ShowWindow(IntPtr p_window_handle, int p_show_command);

		private const int SW_SHOW    = 5;
		private const int SW_HIDE    = 0;
		private const int SW_RESTORE = 9;

		[DllImport("user32.dll")]
		private static extern bool ShowWindowAsync(HandleRef p_window_handle, int p_show_command);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetForegroundWindow(IntPtr p_window_handle);

		[DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
		private static extern IntPtr FindWindowByCaption(IntPtr p_zero_only, string p_window_name);
		
		// ReSharper restore UnusedMember.Local
		#endregion

		/// <summary>The console's title as it was before the game changes it. Used to revert it when the game is finished.</summary>
		private static string original_console_title = string.Empty;
		/// <summary>The console's output encoding format as it was before the game changes it. Used to revert it when the game is finished.</summary>
		private static Encoding original_console_encoding = Encoding.Default;
		// TODO(LOGIX): Summary
		/// <summary></summary>
		internal static bool hold_console_at_end = false;

		internal static bool console_is_hidden = false;

		internal static void Initialize()
		{
			/* Store the console's original properties (any that we intend to change), so that they can be reverted back when the
			 game ends. */
			original_console_title = Console.Title;
			original_console_encoding = Console.OutputEncoding;

			/* Change the console's properties */
			Console.Title = "Terrascape Developer Console / Debugger";
			Console.OutputEncoding = Encoding.UTF8;
			
			/* Determine whether or not to hide the console. The console should be hidden when not running in debug mode, and the
			 game was not run from an existing console. */
			if (!Debug.enable_debugging && !ConsoleWillCloseAtEnd)
			{
				HideConsole();
			}
			
			Console.WriteLine();
			Debug.LogProgram("--------------------[  HELLO!  ]--------------------");
		}

		internal static void Terminate()
		{
			if (console_is_hidden)
			{
				ShowConsole();
			}
			FocusConsole();

			if (Debug.enable_debugging)
			{
				if (hold_console_at_end)
				{
					Debug.LogProgram("Press any key to exit.");
					Console.ReadKey();					
				}
				else
				{
					Debug.LogProgram("Terminating in one second...");
					Thread.Sleep(1000);
				}
			}

			Console.Title = original_console_title;
			Console.OutputEncoding = original_console_encoding;
			Debug.LogProgram("--------------------[ GOODBYE! ]--------------------");
		}

		internal static bool ConsoleWillCloseAtEnd
		{
			get
			{
				uint[] process_list  = new uint[1];
				uint   process_count = GetConsoleProcessList(process_list, 1);
				return process_count == 1;
			}
		}

		internal static void HideConsole()
		{
			IntPtr handle = GetConsoleWindow();
			ShowWindowAsync(new HandleRef(null, handle), SW_HIDE);
			console_is_hidden = true;
		}

		internal static void ShowConsole()
		{
			if (console_is_hidden)
			{
				IntPtr handle = GetConsoleWindow();
				ShowWindowAsync(new HandleRef(null, handle), SW_RESTORE);
			}
		}

		internal static void FocusConsole()
		{
			IntPtr handle = GetConsoleWindow();
			SetForegroundWindow(handle);
		}
	}
}