#nullable enable

namespace Terrascape.Helpers
{
	public static class StringHelper
	{
		public static string AorAn(in string p_string, in bool p_include = true, in bool p_capital = false)
		{
			string lower = p_string.ToLower().Trim();
			bool starts_with_vowel = lower.StartsWith("a") || lower.StartsWith("e") || lower.StartsWith("i") || lower.StartsWith("o") || lower.StartsWith("u");

			return (starts_with_vowel ? (p_capital ? "An" : "an") : (p_capital ? "A" : "a")) + (p_include ? $" {p_string}" : "");
		}
	}
}