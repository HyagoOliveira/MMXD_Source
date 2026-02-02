using UnityEngine;

public class CH049_U_Other_Controller : MonoBehaviour
{
	private enum TalkState
	{
		Idle = 0,
		sheet1 = 1
	}

	private int i_MainTex = Shader.PropertyToID("_MainTex");

	[SerializeField]
	private Renderer talkRenderer;

	[SerializeField]
	private TalkState talkState;

	[SerializeField]
	private Texture textureIdle;

	[SerializeField]
	private Texture[] textureSheet1;

	[SerializeField]
	private float updateRate = 0.17f;

	private MaterialPropertyBlock materialPropertyBlock;

	private int nowIndex;

	private Texture nowTexture;

	private float time;

	private void Awake()
	{
		materialPropertyBlock = new MaterialPropertyBlock();
		nowIndex = 0;
		nowTexture = textureIdle;
		talkRenderer.GetPropertyBlock(materialPropertyBlock);
		materialPropertyBlock.SetTexture(i_MainTex, nowTexture);
		talkRenderer.SetPropertyBlock(materialPropertyBlock);
		Singleton<GenericEventManager>.Instance.AttachEvent<bool, float>(EventManager.ID.CHARACTER_RT_DIALOG, UpdateDialog);
	}

	private void OnDestroy()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent<bool, float>(EventManager.ID.CHARACTER_RT_DIALOG, UpdateDialog);
	}

	private void UpdateDialog(bool isOff, float updateRate)
	{
		if (isOff)
		{
			talkState = TalkState.Idle;
			return;
		}
		talkState = TalkState.sheet1;
		this.updateRate = updateRate;
	}

	private void FixedUpdate()
	{
		time += updateRate;
		switch (talkState)
		{
		case TalkState.Idle:
			if (nowTexture == textureIdle)
			{
				return;
			}
			nowTexture = textureIdle;
			break;
		case TalkState.sheet1:
		{
			float num = time % (float)textureSheet1.Length;
			if ((float)nowIndex == num)
			{
				return;
			}
			nowIndex = (int)num;
			nowTexture = textureSheet1[nowIndex];
			break;
		}
		}
		materialPropertyBlock.SetTexture(i_MainTex, nowTexture);
		talkRenderer.SetPropertyBlock(materialPropertyBlock);
	}
}
