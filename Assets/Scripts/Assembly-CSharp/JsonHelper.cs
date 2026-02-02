using System;
using JsonFx.Json;
using Newtonsoft.Json;

public class JsonHelper
{
	private static readonly bool useJsonFx = true;

	public static string Serialize(object value)
	{
		if (useJsonFx)
		{
			return JsonFx.Json.JsonWriter.Serialize(value);
		}
		return JsonConvert.SerializeObject(value);
	}

	public static T Deserialize<T>(string value)
	{
		if (useJsonFx)
		{
			return JsonFx.Json.JsonReader.Deserialize<T>(value);
		}
		return JsonConvert.DeserializeObject<T>(value);
	}

	public static object Deserialize(string value, Type type)
	{
		if (useJsonFx)
		{
			return JsonFx.Json.JsonReader.Deserialize(value, type);
		}
		return JsonConvert.DeserializeObject(value, type);
	}

	public static bool TryDeserialize<T>(string value, out T obj)
	{
		try
		{
			obj = Deserialize<T>(value);
			return true;
		}
		catch
		{
			obj = default(T);
			return false;
		}
	}

	public static JsonSerializerSettings IgnoreLoopSetting()
	{
		return new JsonSerializerSettings
		{
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore
		};
	}

	public static JsonSerializerSettings OrangeDefaultSetting()
	{
		return new JsonSerializerSettings
		{
			Formatting = Formatting.Indented,
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore
		};
	}
}
