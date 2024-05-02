namespace Logic.Extensions;

using System.Globalization;

public static class EnumExtensions
{
	public static bool HasFlag<T>(this T value, T flag) where T : struct, Enum, IComparable, IConvertible, IFormattable
		=> (Convert.ToUInt64(value, CultureInfo.InvariantCulture) & Convert.ToUInt64(flag, CultureInfo.InvariantCulture)) == 0;
}
