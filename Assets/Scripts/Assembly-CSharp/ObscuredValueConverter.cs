#define RELEASE
using System;
using System.Linq;
using CodeStage.AntiCheat.ObscuredTypes;
using Newtonsoft.Json;

public class ObscuredValueConverter : JsonConverter
{
	private readonly Type[] _types = new Type[14]
	{
		typeof(ObscuredInt),
		typeof(ObscuredFloat),
		typeof(ObscuredDouble),
		typeof(ObscuredDecimal),
		typeof(ObscuredChar),
		typeof(ObscuredByte),
		typeof(ObscuredBool),
		typeof(ObscuredLong),
		typeof(ObscuredSByte),
		typeof(ObscuredShort),
		typeof(ObscuredUInt),
		typeof(ObscuredULong),
		typeof(ObscuredUShort),
		typeof(ObscuredString)
	};

	public override bool CanConvert(Type objectType)
	{
		return _types.Any((Type t) => t == objectType);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		if (value is ObscuredInt)
		{
			writer.WriteValue((ObscuredInt)value);
			return;
		}
		if (value is ObscuredBool)
		{
			writer.WriteValue((ObscuredBool)value);
			return;
		}
		if (value is ObscuredFloat)
		{
			writer.WriteValue((ObscuredFloat)value);
			return;
		}
		Debug.Log("ObscuredValueConverter type " + value.GetType().ToString() + " not implemented");
		writer.WriteValue(value.ToString());
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (reader.Value != null)
		{
			if (objectType == typeof(ObscuredInt))
			{
				return (ObscuredInt)Convert.ToInt32(reader.Value);
			}
			if (objectType == typeof(ObscuredBool))
			{
				return (ObscuredBool)Convert.ToBoolean(reader.Value);
			}
			if (objectType == typeof(ObscuredFloat))
			{
				return (ObscuredFloat)Convert.ToSingle(reader.Value);
			}
			Debug.LogError("Code not implemented yet!");
		}
		return null;
	}
}
