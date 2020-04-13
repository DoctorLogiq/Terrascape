using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Console = Colorful.Console;

#nullable enable

namespace Terrascape.Debugging
{
	public static class Debug
	{
		internal static bool enable_debugging = true;
		internal static bool enable_assertions = true;
		internal static bool enable_profiling = true;
		internal static DebuggingLevel debugging_level = DebuggingLevel.Basic;
		private static int indentation_level = 0;
		private const string INDENTATION_CHAR = "    ";
		private static string current_process = string.Empty;
			
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
		private static void Log(string p_message, DebugChannel p_channel, DebuggingLevel p_min_level, IndentationStyle p_before_indentation, IndentationStyle p_after_indentation)
		{
			if (!enable_debugging) return;
			if (debugging_level < p_min_level) return;

			if (p_before_indentation > 0)
			{
				if (p_before_indentation == IndentationStyle.Indent) Indent();
				else if (p_before_indentation == IndentationStyle.Unindent) Unindent();
				else if (p_before_indentation == IndentationStyle.Reset) ResetIndentation();
			}
			
			string message = $"{p_channel.Tag()} {Timestamp} > {Indentation}{p_message}";
			Console.WriteLine(message, p_channel.Colour());
			
			if (p_after_indentation > 0)
			{
				if (p_after_indentation == IndentationStyle.Indent) Indent();
				else if (p_after_indentation == IndentationStyle.Unindent) Unindent();
				else if (p_after_indentation == IndentationStyle.Reset) ResetIndentation();
			}
		}

		internal static void NewLine()
		{
			string message = $"{DebugChannel.None.Tag()} {EmptyTimestamp} >";
			Console.WriteLine(message, DebugChannel.Debug.Colour());
		}

		internal static void LogDebug(string p_message, DebuggingLevel p_min_level = DebuggingLevel.Basic, IndentationStyle p_before_indentation = IndentationStyle.None, IndentationStyle p_after_indentation = IndentationStyle.None)
		{
			Log(p_message, DebugChannel.Debug, p_min_level, p_before_indentation, p_after_indentation);
		}

		internal static void LogDebugProcessStart(string p_process_name, DebuggingLevel p_min_level = DebuggingLevel.Basic, IndentationStyle p_before_indentation = IndentationStyle.None)
		{
			current_process = p_process_name;
			LogDebug($"{p_process_name}...", p_min_level, p_before_indentation, IndentationStyle.Indent);
		}
		
		internal static void LogDebugProcessEnd(bool p_successful, DebuggingLevel p_min_level = DebuggingLevel.Basic, IndentationStyle p_after_indentation = IndentationStyle.None)
		{
			// TODO(LOGIX): Possibly change this, or use colours
			LogDebug($"{current_process} finished {(p_successful ? "successfully" : "unsuccessfully")}", p_min_level, IndentationStyle.Unindent, p_after_indentation);
		}

		internal static void LogTest(string p_message, DebuggingLevel p_min_level = DebuggingLevel.Basic, IndentationStyle p_before_indentation = IndentationStyle.None, IndentationStyle p_after_indentation = IndentationStyle.None)
		{
			Log(p_message, DebugChannel.Test, p_min_level, p_before_indentation, p_after_indentation);
		}

		internal static void LogProgram(string p_message, DebuggingLevel p_min_level = DebuggingLevel.Basic, IndentationStyle p_before_indentation = IndentationStyle.None, IndentationStyle p_after_indentation = IndentationStyle.None)
		{
			Log(p_message, DebugChannel.Program, p_min_level, p_before_indentation, p_after_indentation);
		}

		internal static void LogInfo(string p_message, DebuggingLevel p_min_level = DebuggingLevel.Basic, IndentationStyle p_before_indentation = IndentationStyle.None, IndentationStyle p_after_indentation = IndentationStyle.None)
		{
			Log(p_message, DebugChannel.Info, p_min_level, p_before_indentation, p_after_indentation);
		}

		internal static void LogWarning(string p_message, DebuggingLevel p_min_level = DebuggingLevel.Basic, IndentationStyle p_before_indentation = IndentationStyle.None, IndentationStyle p_after_indentation = IndentationStyle.None)
		{
			Log(p_message, DebugChannel.Warning, p_min_level, p_before_indentation, p_after_indentation);
		}

		internal static void LogError(string p_message, DebuggingLevel p_min_level = DebuggingLevel.Basic, IndentationStyle p_before_indentation = IndentationStyle.None, IndentationStyle p_after_indentation = IndentationStyle.None)
		{
			Log(p_message, DebugChannel.Error, p_min_level, p_before_indentation, p_after_indentation);
		}

		internal static void LogCritical(string p_message, DebuggingLevel p_min_level = DebuggingLevel.Basic, IndentationStyle p_before_indentation = IndentationStyle.None, IndentationStyle p_after_indentation = IndentationStyle.None)
		{
			Log(p_message, DebugChannel.Critical, p_min_level, p_before_indentation, p_after_indentation);
		}
		
		// ReSharper restore UnusedMember.Global
		#endregion

		// TODO(LOGIX): Implement assertions

		// TODO(LOGIX): Implement profiling
	}

	internal enum IndentationStyle : uint
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
				DebugChannel.Info     => "¦     INFO ¦",
				DebugChannel.Debug    => "¦    DEBUG ¦",
				DebugChannel.Test     => "¦     TEST ¦",
				DebugChannel.Program  => "¦  PROGRAM ¦",
				DebugChannel.Warning  => "¦  WARNING ¦",
				DebugChannel.Error    => "¦    ERROR ¦",
				DebugChannel.Critical => "¦ CRITICAL ¦",
				DebugChannel.None	  => "¦          ¦",
				_                     => throw new ArgumentOutOfRangeException(nameof(p_channel), p_channel, null)
			};
		}

		internal static Color Colour(this DebugChannel p_channel)
		{
			return p_channel switch
			{
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