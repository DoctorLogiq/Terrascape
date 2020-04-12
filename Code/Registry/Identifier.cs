using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

#nullable enable

namespace Terrascape.Registry
{
	public sealed class Identifier : IEquatable<Identifier>, IEquatable<string>
	{
		private static readonly Regex identifier_regex = new Regex(@"^[a-z]{1}[a-z0-9_]{4,}$", RegexOptions.Compiled);
		private readonly string name;

		public Identifier(string p_name)
		{
			if (!identifier_regex.IsMatch(p_name))
				throw new ApplicationException($"Invalid identifier '{p_name}'"); /*TODO(LOGIX): InvalidIdentifierException*/
			
			this.name = p_name;
		}
		
		public override string ToString()
		{
			return this.name;
		}

		public static implicit operator Identifier(string p_name)
		{
			return new Identifier(p_name);
		}

		public static implicit operator string(Identifier p_identifier)
		{
			return p_identifier.name;
		}
		
		#region Equality Checks
		
		public override int GetHashCode()
		{
			int hash = 352033288;
			hash = hash * -1521134295 + this.name.GetHashCode();
			return hash;
		}
		
		[SuppressMessage("ReSharper", "BaseObjectEqualsIsObjectEquals")]
		public override bool Equals(object p_other)
		{
			if (p_other is Identifier other) return Equals(other);
			return base.Equals(p_other);
		}
		
		public bool Equals(Identifier p_other)
		{
			return p_other != null && this.name == p_other.name;
		}

		public bool Equals(string p_other)
		{
			return this.name == p_other;
		}

		public static bool operator ==(Identifier p_id1, Identifier p_id2)
		{
			return p_id2 != null && (p_id1 != null && p_id1.name == p_id2.name);
		}

		public static bool operator !=(Identifier p_id1, Identifier p_id2)
		{
			return p_id2 != null && (p_id1 != null && p_id1.name != p_id2.name);
		}
		
		#endregion
	}
}