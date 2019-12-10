using UnityEngine;

namespace Kernel.Lang.Extension
{
	public static class ExtendVector
	{
		public static bool EqualsEx(this Vector3 a, Vector3 b)
		{
			return IsZero(a - b);
		}

		public static bool EqualsEx(this Vector2 a, Vector2 b)
		{
			return IsZero(a - b);
		}

		public static bool IsZero(this Vector3 a)
		{
			if (a.x.IsZero() && a.y.IsZero() && a.z.IsZero())
			{
				return true;
			}
			return false;
		}

		public static bool IsZero(this Vector2 a)
		{
			if (a.x.IsZero() && a.y.IsZero())
			{
				return true;
			}
			return false;
		}

		public static Vector3 NormalizedWithoutY(this Vector3 a)
		{
			var ret = a;
			ret.y = 0;
			return ret.normalized;
		}

		public static float SqrMagnitudeWithoutY(this Vector3 a)
		{
			return a.x * a.x + a.z * a.z;
		}
	}
}