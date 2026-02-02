using System;
using Newtonsoft.Json;

public class FloatConverter : JsonConverter
{
	private int _precision;

	private MidpointRounding _rounding;

	public override bool CanRead
	{
		get
		{
			return false;
		}
	}

	public FloatConverter()
		: this(3)
	{
	}

	public FloatConverter(int precision)
		: this(precision, MidpointRounding.AwayFromZero)
	{
	}

	public FloatConverter(int precision, MidpointRounding rounding)
	{
		_precision = precision;
		_rounding = rounding;
	}

	public override bool CanConvert(Type objectType)
	{
		return (objectType == typeof(decimal)) | (objectType == typeof(double)) | (objectType == typeof(float));
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		decimal num = Math.Round(Convert.ToDecimal(value), _precision, _rounding).Normalize();
		long result = 0L;
		if (long.TryParse(num.ToString(), out result))
		{
			writer.WriteValue(result);
		}
		else
		{
			writer.WriteValue(num.ToString("G29"));
		}
	}
}
