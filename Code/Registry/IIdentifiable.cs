using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace Terrascape.Registry
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public interface IIdentifiable
	{
		Identifier name { get; }
	}
}