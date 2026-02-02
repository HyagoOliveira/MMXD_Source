using UnityEngine;

public class CH116_EggMeshController : MonoBehaviour
{
	[SerializeField]
	private Texture2D[] _textureEggGift;

	[SerializeField]
	private ParticleSystemRenderer _meshRenderer;

	private MaterialPropertyBlock _materialPropertyBlock;

	private readonly int MAIN_TEXTURE_ID = Shader.PropertyToID("_MainTex");

	public int MeshIndex { get; private set; }

	private void Awake()
	{
		_materialPropertyBlock = new MaterialPropertyBlock();
		_meshRenderer.GetPropertyBlock(_materialPropertyBlock);
	}

	public void SetMeshIndex(int index)
	{
		MeshIndex = index;
		_materialPropertyBlock.SetTexture(MAIN_TEXTURE_ID, _textureEggGift[index]);
		_meshRenderer.SetPropertyBlock(_materialPropertyBlock);
	}
}
