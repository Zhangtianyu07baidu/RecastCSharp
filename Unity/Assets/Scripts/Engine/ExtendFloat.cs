
namespace Kernel.Lang.Extension
{
	public static class ExtendFloat
	{
		public const float EPSILON = 1e-6f;

		public static bool EqualsEx(this float a, float b)
		{
			return IsZero(a - b);
		}

		public static bool IsZero(this float a)
		{
			if (a < EPSILON && a > -EPSILON)
				return true;
			return false;
		}

		public static bool GreaterEx(this float a, float b)
		{
			return a - b > EPSILON;
		}

		public static bool GreaterOrEqualsEx(this float a, float b)
		{
			return a - b > -EPSILON;
		}
	}
}
