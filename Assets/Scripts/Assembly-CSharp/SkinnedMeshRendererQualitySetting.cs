using UnityEngine;

public class SkinnedMeshRendererQualitySetting : MonoBehaviour
{
	[SerializeField]
	private SkinQuality SkinQuality = SkinQuality.Bone4;

	[SerializeField]
	private SkinnedMeshRenderer SkinnedMeshRenderer;

	private void Awake()
	{
		if (!(SkinnedMeshRenderer == null))
		{
			SkinnedMeshRenderer.quality = SkinQuality;
		}
	}
}
