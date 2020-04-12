﻿using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using Terrascape.Debugging;
using Terrascape.Helpers;
using Terrascape.Windowing;

#nullable enable

namespace Terrascape
{
	internal abstract class Program
	{
		private static Program? instance = null;
		
		private const int SAMPLES = 8;
		protected readonly TerrascapeWindow window = new TerrascapeWindow(1280, 720, new GraphicsMode(32, 24, 8, SAMPLES), "Terrascape", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.ForwardCompatible);
		private bool has_shut_down = false;
		private bool is_self_shutting_down = false;

		internal static void Main(string[] p_arguments)
		{
			instance = new Terrascape();
			
			/* Initialize the console, changing its title and encoding mode and preparing it for our use */
			ConsoleHelper.Initialize();
			List<(string, bool)> early_debug_messages = new List<(string, bool)>();

			/* Parse program arguments so we know whether or not to run in debug mode, and what kind of startup options to use */
			#region Argument Parsing
			StartupParameters startup_parameters = new StartupParameters();
			foreach (string argument in p_arguments)
			{
				/* "Enable Debugging" - whether or not to show the console and log debugging information */
				if (argument == startup_parameters.enable_debugging.Item1)
					startup_parameters.enable_debugging.Item2 = true;
				/* "Verbose Debugging Level" - whether or not to include extra debugging information */
				else if (argument == startup_parameters.verbose_debugging.Item1)
					startup_parameters.verbose_debugging.Item2 = true;
				/* "Hold Console" - whether or not to hold the console open if there are no errors when the game quits */
				else if (argument == startup_parameters.hold_console.Item1)
					startup_parameters.hold_console.Item2 = true;
				/* "Enable Assertions" - whether or not to allow Debug.Assert() to throw exceptions */
				else if (argument == startup_parameters.enable_assertions.Item1)
					startup_parameters.enable_assertions.Item2 = true;
				/* "Enable Profiling" - whether or not to allow Debug profiling to record game performance */
				else if (argument == startup_parameters.enable_profiling.Item1)
					startup_parameters.enable_profiling.Item2 = true;
				/* Unknown */
				else
					early_debug_messages.Add(($"Unrecognised argument '{argument}'; ignoring", true));
			}

			/* After parsing arguments, enable the corresponding options */
			if (!startup_parameters.enable_debugging.Item2)
			{
				Debug.enable_debugging = false;
			}

			if (startup_parameters.verbose_debugging.Item2)
			{
				if (Debug.enable_debugging)
				{
					Debug.debugging_level = DebuggingLevel.Verbose;
					early_debug_messages.Add(("Debugging level set to verbose", false));
				}
				else
				{
					early_debug_messages.Add(($"The argument '{startup_parameters.enable_debugging}' requires '-debug' to also be used; this option will be ignored", true));
				}
			}

			if (startup_parameters.hold_console.Item2)
			{
				if (Debug.enable_debugging)
				{
					ConsoleHelper.hold_console_at_end = true;
					early_debug_messages.Add(("Console will be held at game end", false));
				}
				else
				{
					early_debug_messages.Add(($"The argument '{startup_parameters.hold_console}' requires '-debug' to also be used; this option will be ignored", true));
				}
			}
			
			if (startup_parameters.enable_assertions.Item2)
			{
				if (Debug.enable_debugging)
				{
					Debug.enable_assertions = true;
					early_debug_messages.Add(("Assertions enabled", false));
				}
				else
				{
					early_debug_messages.Add(($"The argument '{startup_parameters.enable_assertions.Item1}' requires '-debug' to also be used; this option will be ignored", true));
				}
			}
			
			if (startup_parameters.enable_profiling.Item2)
			{
				if (Debug.enable_debugging)
				{
					Debug.enable_profiling = true;
					early_debug_messages.Add(("Profiling enabled", false));
				}
				else
				{
					early_debug_messages.Add(($"The argument '{startup_parameters.enable_profiling.Item1}' requires '-debug' to also be used; this option will be ignored", true));
				}
			}

			#endregion
			
			/* Now print any messages that were supposed to be printed during argument parsing. This is held
			 until now because we first need to know whether or not debugging mode is going to be enabled. */
			Debug.LogDebugProcessStart("Parsing arguments");
#pragma warning disable CS8619
			foreach ((string message, bool is_warning) in early_debug_messages)
			{
				if (is_warning) Debug.LogWarning(message);
				else Debug.LogDebug($"{SpecialCharacters.Bullet} {message}");
			}
#pragma warning restore CS8619
			Debug.LogDebugProcessEnd(true);
			early_debug_messages.Clear();

			/* Run the game! */
			#region Window Setup
			instance.window.Load += (p_sender, p_args) =>
			{
				instance.Initialize();
				instance.Load();
			};
			instance.window.UpdateFrame += (p_sender, p_args) =>
			{
				instance.Update(p_args.Time);
			};
			instance.window.RenderFrame += (p_sender, p_args) =>
			{
				instance.Render(p_args.Time);
			};
			instance.window.Resize += (p_sender, p_args) =>
			{
				instance.Resize();
			};
			instance.window.Unload += (p_sender, p_args) =>
			{
				if (instance.has_shut_down) return;
				instance.Shutdown();
				instance.has_shut_down = true;
			};
			instance.window.Closing += (p_sender, p_args) =>
			{
				if (instance.has_shut_down) return;
				
				if (!instance.is_self_shutting_down)
				{
					bool cancel = false;
					instance.RequestShutdown(ref cancel);
					if (!cancel)
					{
						instance.Shutdown();
						instance.has_shut_down = true;
					}
				}
			};
			#endregion
			instance.window.Run();

			// Reset the console back to its previous state as it was before the game started, and hold it open if needed
			ConsoleHelper.Terminate();
		}

		protected abstract void Initialize();
		protected abstract void Load();
		protected abstract void Update(in double p_delta);
		protected abstract void Render(in double p_delta);
		protected abstract void Resize();
		protected abstract void RequestShutdown(ref bool p_cancel);
		protected abstract void Shutdown();
	}
}