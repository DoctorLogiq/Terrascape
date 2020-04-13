using System;
using System.IO;
using System.Net;

#nullable enable

namespace Terrascape.Helpers
{
	internal static class GitHelper
	{
		internal static string? ReadResource(in string p_url, bool p_exception_on_fail = false)
		{
			string? content = null;
			try
			{
				WebClient    client = new WebClient();
				Stream       stream = client.OpenRead(p_url);
				StreamReader reader = new StreamReader(stream);
				content = reader.ReadToEnd();
				content = content.Replace("\n", Environment.NewLine);
			}
			catch (Exception)
			{
				if (p_exception_on_fail)
					throw;
			}

			return content;
		}
	}
}