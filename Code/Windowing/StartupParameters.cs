
#nullable enable

namespace Terrascape.Windowing
{
	public class StartupParameters
	{
		internal (string, bool) enable_debugging  = ("-debug", false);
		internal (string, bool) verbose_debugging = ("-verbose", false);
		internal (string, bool) hold_console      = ("-holdConsole", false);
		internal (string, bool) enable_assertions = ("-unsafe", false);
		internal (string, bool) enable_profiling  = ("-profile", false);
	}
}