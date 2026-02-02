using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TextureList
{
	public List<Texture2D> list = new List<Texture2D>();

	public Texture2D texture
	{
		get
		{
			return list[UnityEngine.Random.Range(0, list.Count)];
		}
	}

	public Texture2D this[int index]
	{
		get
		{
			return list[index];
		}
		set
		{
			list[index] = value;
		}
	}

	public int Count
	{
		get
		{
			return list.Count;
		}
	}

	public TextureList()
	{
		list = new List<Texture2D>();
	}

	public void Add(Texture2D tex)
	{
		list.Add(tex);
	}

	public void Remove(Texture2D tex)
	{
		list.Remove(tex);
	}

	public void RemoveAt(int index)
	{
		list.RemoveAt(index);
	}

	public void Clear()
	{
		list.Clear();
	}
}
