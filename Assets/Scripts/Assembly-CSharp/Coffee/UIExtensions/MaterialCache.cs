using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Coffee.UIExtensions
{
	public class MaterialCache
	{
		public static List<MaterialCache> materialCaches = new List<MaterialCache>();

		public ulong hash { get; private set; }

		public int referenceCount { get; private set; }

		public Texture texture { get; private set; }

		public Material material { get; private set; }

		public static MaterialCache Register(ulong hash, Texture texture, Func<Material> onCreateMaterial)
		{
			MaterialCache materialCache = materialCaches.FirstOrDefault((MaterialCache x) => x.hash == hash);
			if (materialCache != null && (bool)materialCache.material)
			{
				if ((bool)materialCache.material)
				{
					materialCache.referenceCount++;
				}
				else
				{
					materialCaches.Remove(materialCache);
					materialCache = null;
				}
			}
			if (materialCache == null)
			{
				materialCache = new MaterialCache
				{
					hash = hash,
					material = onCreateMaterial(),
					referenceCount = 1
				};
				materialCaches.Add(materialCache);
			}
			return materialCache;
		}

		public static MaterialCache Register(ulong hash, Func<Material> onCreateMaterial)
		{
			MaterialCache materialCache = materialCaches.FirstOrDefault((MaterialCache x) => x.hash == hash);
			if (materialCache != null)
			{
				materialCache.referenceCount++;
			}
			if (materialCache == null)
			{
				materialCache = new MaterialCache
				{
					hash = hash,
					material = onCreateMaterial(),
					referenceCount = 1
				};
				materialCaches.Add(materialCache);
			}
			return materialCache;
		}

		public static void Unregister(MaterialCache cache)
		{
			if (cache != null)
			{
				cache.referenceCount--;
				if (cache.referenceCount <= 0)
				{
					materialCaches.Remove(cache);
					cache.material = null;
				}
			}
		}
	}
}
