using UnityEngine;

public class GachaReward : MonoBehaviour
{
	private readonly int i_Phase = Shader.PropertyToID("_Phase");

	private readonly int i_Color = Shader.PropertyToID("_Color");

	[SerializeField]
	private MeshRenderer mesh;

	[SerializeField]
	private Material materialRef;

	private Material stMaterial;

	[SerializeField]
	private Color[] rareColor = new Color[7];

	private void Awake()
	{
		stMaterial = Object.Instantiate(materialRef);
		mesh.material = stMaterial;
		stMaterial.SetFloat(i_Phase, 0f);
	}

	public void ShowResult(int rare, float delay = 0f)
	{
		stMaterial.SetColor(i_Color, rareColor[rare]);
		stMaterial.SetFloat(i_Phase, 0f);
		LeanTween.value(base.gameObject, 0f, 1f, 2f).setOnUpdate(delegate(float val)
		{
			stMaterial.SetFloat(i_Phase, val);
		}).setDelay(delay);
	}

	public void SetVisable(int val = 1)
	{
		LeanTween.cancel(base.gameObject);
		stMaterial.SetFloat(i_Phase, val);
	}

	private void OnDisable()
	{
		if (stMaterial != null)
		{
			Object.Destroy(stMaterial);
		}
	}
}
