using UnityEngine;

public class SCH016CustomBody : MonoBehaviour
{
	[SerializeField]
	private SkinnedMeshRenderer[] _SkinnedMeshRendererBody = new SkinnedMeshRenderer[0];

	public void SetVisible(bool _visible)
	{
		SkinnedMeshRenderer[] skinnedMeshRendererBody = _SkinnedMeshRendererBody;
		for (int i = 0; i < skinnedMeshRendererBody.Length; i++)
		{
			skinnedMeshRendererBody[i].enabled = _visible;
		}
	}
}
