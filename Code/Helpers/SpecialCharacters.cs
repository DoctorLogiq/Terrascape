using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace Terrascape.Helpers
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public class SpecialCharacters
	{
		// Legal
		public const char Trademark  = '™';
		public const char Copyright  = '©';
		public const char Registered = '®';

		// Punctuation
		public const char Ellipsis         = '…';
		public const char Bullet           = '•';
		public const char EnDash           = '–';
		public const char EmDash           = '—';
		public const char Section          = '§';
		public const char BrokenBar        = '¦';
		public const char Interpunct       = '·';
		public const char DoubleRightAngle = '»';
		public const char DoubleLeftAngle  = '«';

		// Mathematical
		public const char Squared            = '²';
		public const char Cubed              = '³';
		public const char Half               = '½';
		public const char Quarter            = '¼';
		public const char Degree             = '°';
		public const char PlusMinus          = '±';
		public const char Percent            = '%';
		public const char Multiply           = '×';
		public const char Divide             = '÷';
		public const char ExactlyIdentical   = '≡';
		public const char ApproximatelyEqual = '≈';
		public const char GreaterOrEqual     = '≥';
		public const char LessOrEqual        = '≤';
		public const char Pi                 = 'π';
		public const char Infinity           = '∞';
		public const char SquareRoot         = '√';
		public const char Sum                = 'Σ';
		public const char Function           = 'ƒ';
		public const char Null               = 'ø';
		public const char Null2              = 'Ø';
	}
}