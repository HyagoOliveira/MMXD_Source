using UnityEngine;

public class BS055_Controller : BS034_Controller
{
	protected override void Awake()
	{
		base.Awake();
		IsBigBoss = false;
		MonoBehaviourSingleton<OrangeBattleUtility>.Instance.ChangeRenderLayer(RenderModes, ManagedSingleton<OrangeLayerManager>.Instance.EnemyLayer);
	}

	protected override void releaseOC()
	{
		if (IsCatch && (bool)targetOC)
		{
			targetOC.transform.position = new Vector3(targetOC.transform.position.x, targetOC.transform.position.y, 0f);
			targetOC.Controller.LogicPosition = new VInt3(targetOC._transform.position);
			targetOC.ModelTransform.rotation = _originRotation;
			if ((int)targetOC.Hp > 0)
			{
				targetOC.SetStun(false);
			}
			playPullWallVFX();
		}
		MonoBehaviourSingleton<OrangeBattleUtility>.Instance.ChangeRenderLayer(RenderModes, ManagedSingleton<OrangeLayerManager>.Instance.EnemyLayer);
		targetOC = null;
		IsCatch = false;
	}
}
