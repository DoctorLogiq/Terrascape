using System;
using System.IO;
using Terrascape.Debugging;
using Terrascape.Exceptions;

#nullable enable

namespace Terrascape.Helpers
{
	internal static class AssetHelper
	{
		internal static string ReadTextAsset(string p_filename, bool p_exception_if_not_found = true)
		{
			string filename  = $"{Directory.GetCurrentDirectory()}/Assets/{p_filename}";

			if (!File.Exists(filename))
			{
				if (p_exception_if_not_found)
					throw new TerrascapeException($"Could not read text asset '{filename}'"); // TODO(LOGIX): Custom exception type, better message
				Debug.LogError($"Could not read text asset '{filename}'");
			}

			return File.ReadAllText(filename);;
		}
	}
}