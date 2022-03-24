using System;
using System.Globalization;

namespace Razorpay.Api.Errors
{
	public class ErrorCodeHelper
	{
		public static BaseError Get(string message, string code, int httpStatusCode, string field)
		{
			string text = string.Join(" ", code.Split(new char[]
			{
				'_'
			}));
			text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());
			text = string.Join("", text.Split(new char[]
			{
				' '
			}));
			text = "Razorpay.Api.Errors." + text;
			Type type = Type.GetType(text);
			BaseError result;
			if (!string.IsNullOrWhiteSpace(field))
			{
				result = (BaseError)Activator.CreateInstance(type, new object[]
				{
					message,
					code,
					httpStatusCode,
					field
				});
			}
			else
			{
				result = (BaseError)Activator.CreateInstance(type, new object[]
				{
					message,
					code,
					httpStatusCode
				});
			}
			return result;
		}
	}
}
