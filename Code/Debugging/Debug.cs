using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq.Expressions;
using Colorful;
using Terrascape.Exceptions;
using Console = Colorful.Console;

#nullable enable

namespace Terrascape.Debugging
{
	public static class Debug
	{
		// TODO(LOGIX): Create a log file
		internal static bool enable_debugging = true;
		internal static bool enable_assertions = true;
		internal static bool enable_profiling = true;
		internal static DebuggingLevel debugging_level = DebuggingLevel.Basic;
		private static int indentation_level = 0;
		private const string INDENTATION_CHAR = "    ";
		private static string current_process = string.Empty;
		
		private static readonly StyleSheet debug_style_sheet = new StyleSheet(DebugChannel.Debug.Colour());

		static Debug()
		{
			// Add syntax highlighting to numbers and identifiers, courtesy of ColorfulConsole!
			debug_style_sheet.AddStyle(@"(?<=')[a-z0-9_]+(?=')", Color.SeaGreen);
			debug_style_sheet.AddStyle(@"(?<=.{29})[-+.]?\d+([\.:,]\d+)*", Color.DodgerBlue);
		}
			
		private static string Timestamp
		{
			get
			{
				DateTime now = DateTime.Now;
				return $"{now.Hour:00}:{now.Minute:00}:{now.Second:00}::{now.Millisecond:000}";
			}
		}
		
		private static string EmptyTimestamp => "             ";

		#region Indentation

		private static string Indentation { get; set; } = string.Empty;

		internal static void Indent(DebuggingLevel p_min_level = DebuggingLevel.Basic)
		{
			if (debugging_level < p_min_level) return;
			
			indentation_level++;
			Indentation += INDENTATION_CHAR;
		}
		
		internal static void Unindent(DebuggingLevel p_min_level = DebuggingLevel.Basic)
		{
			if (debugging_level < p_min_level) return;

			if (--indentation_level < 0)
				indentation_level = 0;
			
			Indentation = string.Empty;
			
			for (int i = 0; i < indentation_level; ++i)
				Indentation += INDENTATION_CHAR;
		}

		internal static void ResetIndentation(DebuggingLevel p_min_level = DebuggingLevel.Basic)
		{
			if (debugging_level < p_min_level) return;
			if (indentation_level == 0) return;

			indentation_level = 0;
			Indentation = string.Empty;
		}
		
		#endregion
		
		#region Logging
		// ReSharper disable UnusedMember.Global

		[SuppressMessage("ReSharper", "ConvertIfStatementToSwitchStatement")]
		[SuppressMessage("ReSharper", "InvertIf")]
		private static void Log(string p_message, DebugChannel p_channel, DebuggingLevel p_min_level, Indentation p_pre, Indentation p_post)
		{
			if (!enable_debugging && p_channel == DebugChannel.Debug) return;
			if (debugging_level < p_min_level) return;

			// If the pre-indentation style is not set to none, perform the necessary indentation/unindentation/reset task first
			if (p_pre > 0)
			{
				if (p_pre == Debugging.Indentation.Indent) Indent();
				else if (p_pre == Debugging.Indentation.Unindent) Unindent();
				else if (p_pre == Debugging.Indentation.Reset) ResetIndentation();
			}
			
			// Re-format the message to include the channel tag, timestamp and indentation
			string message = $"{p_channel.Tag()} {Timestamp} > {Indentation}{p_message}";
			
			// Print the message to the console using the correct colours
			if (p_channel != DebugChannel.Debug)
			{
				
				Console.WriteLine(message, p_channel.Colour());
			}
			else
			{
				// If using the [debug] channel, use the debug stylesheet to allow syntax highlighting
				Console.WriteLineStyled(message, debug_style_sheet);
			}

			// If the post-indentation style is not set to none, perform the necessary indentation/unindentation/reset task
			if (p_post > 0)
			{
				if (p_post == Debugging.Indentation.Indent) Indent();
				else if (p_post == Debugging.Indentation.Unindent) Unindent();
				else if (p_post == Debugging.Indentation.Reset) ResetIndentation();
			}
		}

		internal static void NewLine()
		{
			string message = $"{DebugChannel.None.Tag()} {EmptyTimestamp} >";
			Console.WriteLine(message, DebugChannel.Debug.Colour());
		}

		internal static void LogDebug(string p_message, DebuggingLevel p_min_level = DebuggingLevel.Basic, Indentation p_pre = Debugging.Indentation.None, Indentation p_post = Debugging.Indentation.None)
		{
			Log(p_message, DebugChannel.Debug, p_min_level, p_pre, p_post);
		}

		/*internal static void LogDebugProcessStart(string p_process_name, DebuggingLevel p_min_level = DebuggingLevel.Basic, Indentation p_before = Debugging.Indentation.None)
		{
			current_process = p_process_name;
			LogDebug($"{p_process_name}...", p_min_level, p_before, Debugging.Indentation.Indent);
		}
		
		internal static void LogDebugProcessEnd(bool p_successful, DebuggingLevel p_min_level = DebuggingLevel.Basic, Indentation p_after = Debugging.Indentation.None)
		{
			LogDebug($"{current_process} finished {(p_successful ? "successfully" : "unsuccessfully")}", p_min_level, Debugging.Indentation.Unindent, p_after);
		}*/

		internal static void LogTest(string p_message, DebuggingLevel p_min_level = DebuggingLevel.Basic, Indentation p_pre = Debugging.Indentation.None, Indentation p_post = Debugging.Indentation.None)
		{
			Log(p_message, DebugChannel.Test, p_min_level, p_pre, p_post);
		}

		internal static void LogProgram(string p_message, DebuggingLevel p_min_level = DebuggingLevel.Basic, Indentation p_pre = Debugging.Indentation.None, Indentation p_post = Debugging.Indentation.None)
		{
			Log(p_message, DebugChannel.Program, p_min_level, p_pre, p_post);
		}

		internal static void LogInfo(string p_message, DebuggingLevel p_min_level = DebuggingLevel.Basic, Indentation p_pre = Debugging.Indentation.None, Indentation p_post = Debugging.Indentation.None)
		{
			Log(p_message, DebugChannel.Info, p_min_level, p_pre, p_post);
		}

		internal static void LogWarning(string p_message, DebuggingLevel p_min_level = DebuggingLevel.Basic, Indentation p_pre = Debugging.Indentation.None, Indentation p_post = Debugging.Indentation.None)
		{
			Log(p_message, DebugChannel.Warning, p_min_level, p_pre, p_post);
		}

		internal static void LogError(string p_message, DebuggingLevel p_min_level = DebuggingLevel.Basic, Indentation p_pre = Debugging.Indentation.None, Indentation p_post = Debugging.Indentation.None)
		{
			Log(p_message, DebugChannel.Error, p_min_level, p_pre, p_post);
		}

		internal static void LogCritical(string p_message, DebuggingLevel p_min_level = DebuggingLevel.Basic, Indentation p_pre = Debugging.Indentation.None, Indentation p_post = Debugging.Indentation.None)
		{
			Log(p_message, DebugChannel.Critical, p_min_level, p_pre, p_post);
		}
		
		// ReSharper restore UnusedMember.Global
		#endregion
		
		#region Assertions
		
		/// <summary>
		/// Allows you to assert an expression. The expression must be passed as a lambda. If the expression does not pass,
		/// an exception will be thrown.
		/// </summary>
		/// <param name="p_expression">The expression in which you are asserting should be true.</param>
		/// <param name="p_illegal_state">Whether to throw an IllegalStateException instead of a TerrascapeException.</param>
		/// <param name="p_message">The exception message. Accepts the parameter specifier %EXPR% which is the expression as a string.</param>
		/// <exception cref="IllegalStateException">Thrown if the expression does not pass and p_illegal_state is true.</exception>
		/// <exception cref="TerrascapeException">Thrown if the expression does not pass and p_illegal_state is false.</exception>
		public static void Assert(Expression<Func<bool>> p_expression, bool p_illegal_state = false, string p_message = "Assertion failed: %EXPR%")
		{
			if (!enable_assertions) return;
			
			bool pass = p_expression.Compile().Invoke();
			if (!pass)
			{
				string expression_as_string = p_expression.Body.ToString().Substring(1, p_expression.Body.ToString().Length - 2);
				if (p_illegal_state)
					throw new IllegalStateException(p_message.Replace("%EXPR%", expression_as_string));
				else
					throw new TerrascapeException(p_message.Replace("%EXPR%", expression_as_string));
			}
		}
		
		/// <summary>
		/// Allows you to assert an expression, however if the expression fails, the code will still continue as an
		/// exception will not be thrown (a warning/error will be logged instead).
		/// <para>
		/// Returned tuple: Item1 is whether or not the function was run (this will be false if assertions are not enabled).
		/// Item2 is the actual result of the expression test, IF the function was run. This will always be true if the function was not run.
		/// </para>
		/// </summary>
		/// <param name="p_expression">The expression in which you are asserting should be true.</param>
		/// <param name="p_logging_mode">The debug channel in which to log the warning in the case that the expression did not pass.</param>
		/// <returns>Tuple: Item1 is whether or not the function was run (this will be false if assertions are not enabled).
		/// Item2 is the actual result of the expression test, IF the function was run.</returns>
		public static (bool, bool) SafeUnsafeAssert(Expression<Func<bool>> p_expression, AssertionLoggingMode p_logging_mode = AssertionLoggingMode.Error)
		{
			if (!enable_assertions) return (false, true);
			
			bool pass = p_expression.Compile().Invoke();
			string expression_as_string = p_expression.Body.ToString();
			
			if (!pass)
			{
				switch (p_logging_mode)
				{
					case AssertionLoggingMode.Silent:
						return (true, false);
					case AssertionLoggingMode.Warning:
						LogWarning($"Assertion failed: [{expression_as_string}]");
						break;
					case AssertionLoggingMode.Error:
						LogError($"Assertion failed: [{expression_as_string}]");
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(p_logging_mode), p_logging_mode, null);
				}
			}

			return (true, pass);
		}

		/// <summary>
		/// Performs the given task if an assertion passes.
		/// </summary>
		/// <param name="p_expression">The expression in which you are asserting should be true.</param>
		/// <param name="p_task"></param>
		/// <param name="p_always_require_assertion">Whether or not to require the SafeUnsafeAssert function to actually have run.
		/// This means that if assertions are not enabled explicitly by the user, the task will not run. If set to false and assertions
		/// are not enabled, the task will always run.</param>
		public static void DoIfAssertionPasses(Expression<Func<bool>> p_expression, Action p_task, bool p_always_require_assertion = true)
		{
			(bool function_was_run, bool assertion_passed) = SafeUnsafeAssert(p_expression, AssertionLoggingMode.Silent);
			if (p_always_require_assertion && function_was_run && assertion_passed)
			{
				p_task.Invoke();
			}
			else if (!p_always_require_assertion && assertion_passed)
			{
				p_task.Invoke();
			}
		}
		
		#endregion

		// TODO(LOGIX): Implement profiling
	}

	internal enum Indentation : uint
	{
		None = 0,
		Indent,
		Unindent,
		Reset
	}

	internal enum DebugChannel
	{
		Info,
		Debug,
		Test,
		Program,
		Warning,
		Error,
		Critical,
		None
	}

	public enum AssertionLoggingMode
	{
		Silent,
		Warning,
		Error
	}

	internal enum DebuggingLevel : uint
	{
		Basic = 0,
		Verbose = 1
	}

	internal static class DebugChannelExtensions
	{
		internal static string Tag(this DebugChannel p_channel)
		{
			return p_channel switch
			{
				DebugChannel.None     => "¦          ¦",
				DebugChannel.Info     => "¦     INFO ¦",
				DebugChannel.Debug    => "¦    DEBUG ¦",
				DebugChannel.Test     => "¦     TEST ¦",
				DebugChannel.Program  => "¦  PROGRAM ¦",
				DebugChannel.Warning  => "¦  WARNING ¦",
				DebugChannel.Error    => "¦    ERROR ¦",
				DebugChannel.Critical => "¦ CRITICAL ¦",
				_                     => throw new ArgumentOutOfRangeException(nameof(p_channel), p_channel, null)
			};
		}

		internal static Color Colour(this DebugChannel p_channel)
		{
			return p_channel switch
			{
				DebugChannel.None     => Color.Black,
				DebugChannel.Info     => Color.Silver,
				DebugChannel.Debug    => Color.Gray,
				DebugChannel.Test     => Color.Gold,
				DebugChannel.Program  => Color.BlueViolet,
				DebugChannel.Warning  => Color.Goldenrod,
				DebugChannel.Error    => Color.IndianRed,
				DebugChannel.Critical => Color.Red,
				_                     => throw new ArgumentOutOfRangeException(nameof(p_channel), p_channel, null)
			};
		}
	}
}