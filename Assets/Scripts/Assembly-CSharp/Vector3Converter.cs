using System;
using Newtonsoft.Json;
using UnityEngine;

public class Vector3Converter : JsonConverter
{
	private MidpointRounding _rounding = MidpointRounding.AwayFromZero;

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Vector3);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		Vector3 vector = (Vector3)value;
		writer.WriteStartObject();
		writer.WritePropertyName("x");
		WriteValue(writer, vector.x);
		writer.WritePropertyName("y");
		WriteValue(writer, vector.y);
		writer.WritePropertyName("z");
		WriteValue(writer, vector.z);
		writer.WriteEndObject();
	}

	public void WriteValue(JsonWriter writer, float val)
	{
		decimal num = Math.Round(Convert.ToDecimal(val), 3, _rounding).Normalize();
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
