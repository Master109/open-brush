using System;

namespace Extensions
{
	public static class EnumExtensions
	{
		public static bool HasFlag<T>(this T flags, T flag) where T : struct, IConvertible
		{
			var iFlags = Convert.ToUInt64(flags);
			var iFlag = Convert.ToUInt64(flag);
			return ((iFlags & iFlag) == iFlag);
		}
	}
}