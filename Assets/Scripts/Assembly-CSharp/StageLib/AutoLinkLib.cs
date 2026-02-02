using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace StageLib
{
	public class AutoLinkLib
	{
		public static void AutoLinkVar(object target, Type tType, Transform tTrans)
		{
			FieldInfo[] fields = tType.GetFields();
			foreach (FieldInfo fieldInfo in fields)
			{
				if (fieldInfo.FieldType.IsArray)
				{
					FindAndLinkArray(target, tTrans, fieldInfo);
				}
				else
				{
					FindAndLink(target, tTrans, fieldInfo);
				}
			}
		}

		public static void FindAndLink(object target, Transform tTrans, FieldInfo fi)
		{
			if (tTrans == null)
			{
				return;
			}
			Transform transform = FindName(tTrans, fi.Name, false);
			if (transform != null)
			{
				if (fi.FieldType == typeof(GameObject))
				{
					fi.SetValue(target, transform.gameObject);
				}
				else if (transform.GetComponent(fi.FieldType) != null)
				{
					fi.SetValue(target, transform.GetComponent(fi.FieldType));
				}
			}
		}

		public static void FindAndLinkArray(object target, Transform tTrans, FieldInfo fi)
		{
			if (tTrans == null)
			{
				return;
			}
			string arg = fi.FieldType.FullName.Substring(0, fi.FieldType.FullName.Length - 2);
			Type type = Type.GetType(string.Format("{0},{1}", arg, fi.FieldType.Assembly.GetName().Name));
			ArrayList arrayList = new ArrayList();
			new List<Component>();
			Transform transform = null;
			int num = 0;
			while (true)
			{
				transform = FindName(tTrans, fi.Name + num, false);
				if (transform == null)
				{
					break;
				}
				if (type == typeof(GameObject))
				{
					arrayList.Add(transform.gameObject);
				}
				else if (transform.GetComponent(type) != null)
				{
					arrayList.Add(transform.GetComponent(type));
				}
				num++;
			}
			fi.SetValue(target, arrayList.ToArray(type));
		}

		public static Transform FindName(Transform tTrans, string fname, bool bFirstLayer)
		{
			if (tTrans == null)
			{
				return null;
			}
			Transform transform = tTrans.Find(fname);
			if (transform != null)
			{
				return transform;
			}
			if (bFirstLayer)
			{
				return null;
			}
			for (int i = 0; i < tTrans.childCount; i++)
			{
				transform = tTrans.GetChild(i);
				if (transform.childCount > 0)
				{
					transform = FindName(transform, fname, bFirstLayer);
					if (transform != null)
					{
						return transform;
					}
				}
			}
			return null;
		}

		public static T[] GetArraysLink<T>(Transform tTrans, string prename, bool bFirstLayer)
		{
			List<T> list = new List<T>();
			int num = 0;
			while (true)
			{
				string[] array = string.Format(prename, num).Split('/');
				Transform transform = null;
				if (array.Length > 1)
				{
					transform = tTrans;
					for (int i = 0; i < array.Length; i++)
					{
						transform = transform.Find(array[i]);
						if (transform == null)
						{
							break;
						}
					}
				}
				else
				{
					transform = FindName(tTrans, array[0], bFirstLayer);
				}
				if (!(transform != null))
				{
					break;
				}
				T component = transform.GetComponent<T>();
				if (component != null)
				{
					list.Add(component);
				}
				num++;
			}
			if (list.Count > 0)
			{
				return list.ToArray();
			}
			return null;
		}

		public static GameObject[] GetArraysLinkObj(Transform tTrans, string prename, bool bFirstLayer)
		{
			List<GameObject> list = new List<GameObject>();
			int num = 0;
			while (true)
			{
				string[] array = string.Format(prename, num).Split('/');
				Transform transform = null;
				transform = ((array.Length <= 1) ? FindName(tTrans, array[0], bFirstLayer) : tTrans.Find(string.Format(prename, num)));
				if (!(transform != null))
				{
					break;
				}
				list.Add(transform.gameObject);
				num++;
			}
			return list.ToArray();
		}

		public static void DivFloatDecimalPoint(ref float f, ref float fDecPt, int n)
		{
			float num = 1f;
			float num2 = 1f;
			while (n > 0)
			{
				num *= 10f;
				num2 *= 0.1f;
				n--;
			}
			fDecPt = f * num;
			num = Mathf.Floor(fDecPt);
			f = num * num2;
			fDecPt = (fDecPt - num) * num2;
		}

		public static void DivVector3DecimalPoint(ref Vector3 v, ref Vector3 vDecPt, int n)
		{
			DivFloatDecimalPoint(ref v.x, ref vDecPt.x, n);
			DivFloatDecimalPoint(ref v.y, ref vDecPt.y, n);
			DivFloatDecimalPoint(ref v.z, ref vDecPt.z, n);
		}
	}
}
