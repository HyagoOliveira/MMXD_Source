using System;
using System.Reflection;
using CodeStage.AntiCheat.ObscuredTypes;

public class ObscuredConverter : RowDataConverter
{
	public override object Convert(PropertyInfo p, object obj)
	{
		Type propertyType = p.PropertyType;
		if (propertyType == typeof(ObscuredInt))
		{
			obj = (ObscuredInt)System.Convert.ToInt32(obj);
		}
		else if (propertyType == typeof(ObscuredFloat))
		{
			obj = (ObscuredFloat)System.Convert.ToSingle(obj);
		}
		return obj;
	}
}
