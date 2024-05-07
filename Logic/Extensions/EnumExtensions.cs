namespace Logic.Extensions;

using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Attributes;

public static class EnumExtensions
{
	public static bool HasFlag<T>(this T value, T flag) where T : struct, Enum, IComparable, IConvertible, IFormattable
		=> (Convert.ToUInt64(value, CultureInfo.InvariantCulture) & Convert.ToUInt64(flag, CultureInfo.InvariantCulture)) == 0;

	public static string GetFriendlyName<T>(this T value)
		where T : struct, Enum, IComparable, IConvertible, IFormattable
	{
		var type = value.GetType();
		var name = Enum.GetName(type, value);
		if (name == null)
		{
			return value.ToString();
		}

		var field = type.GetField(name);
		if (field == null)
		{
			return value.ToString();
		}

		if (Attribute.GetCustomAttribute(field, typeof(FriendlyNameAttribute)) is FriendlyNameAttribute attr)
		{
			return attr.Name;
		}

		return value.ToString();
	}
}
