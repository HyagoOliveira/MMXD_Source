using UnityEngine;

namespace MagicaCloth
{
	public abstract class WindComponent : MonoBehaviour
	{
		protected int windId = -1;

		protected RuntimeStatus status = new RuntimeStatus();

		public RuntimeStatus Status
		{
			get
			{
				return status;
			}
		}

		protected virtual void Start()
		{
			Init();
		}

		public virtual void OnEnable()
		{
			status.SetEnable(true);
			status.UpdateStatus();
		}

		public virtual void OnDisable()
		{
			status.SetEnable(false);
			status.UpdateStatus();
		}

		protected virtual void OnDestroy()
		{
			OnDispose();
			status.SetDispose();
		}

		protected virtual void Update()
		{
			if (status.IsInitSuccess)
			{
				bool runtimeError = !VerifyData();
				status.SetRuntimeError(runtimeError);
				status.UpdateStatus();
				if (status.IsActive)
				{
					OnUpdate();
				}
			}
		}

		private void Init()
		{
			status.updateStatusAction = OnUpdateStatus;
			if (status.IsInitComplete || status.IsInitStart)
			{
				return;
			}
			status.SetInitStart();
			if (!VerifyData())
			{
				status.SetInitError();
				return;
			}
			OnInit();
			if (!status.IsInitError)
			{
				status.SetInitComplete();
				status.UpdateStatus();
			}
		}

		protected void OnUpdateStatus()
		{
			if (status.IsActive)
			{
				OnActive();
			}
			else
			{
				OnInactive();
			}
		}

		public virtual bool VerifyData()
		{
			return true;
		}

		protected virtual void OnInit()
		{
			CreateWind();
			if (Status.IsActive)
			{
				EnableWind();
			}
		}

		protected virtual void OnDispose()
		{
			if (CreateSingleton<MagicaPhysicsManager>.IsInstance())
			{
				RemoveWind();
			}
		}

		protected virtual void OnUpdate()
		{
		}

		protected virtual void OnActive()
		{
			EnableWind();
		}

		protected virtual void OnInactive()
		{
			DisableWind();
		}

		protected void EnableWind()
		{
			if (windId >= 0)
			{
				CreateSingleton<MagicaPhysicsManager>.Instance.Wind.SetEnable(windId, true, base.transform);
			}
		}

		protected void DisableWind()
		{
			if (CreateSingleton<MagicaPhysicsManager>.IsInstance() && windId >= 0)
			{
				CreateSingleton<MagicaPhysicsManager>.Instance.Wind.SetEnable(windId, false, base.transform);
			}
		}

		protected void RemoveWind()
		{
			if (CreateSingleton<MagicaPhysicsManager>.IsInstance() && windId >= 0)
			{
				CreateSingleton<MagicaPhysicsManager>.Instance.Wind.RemoveWind(windId);
			}
			windId = -1;
		}

		protected abstract void CreateWind();
	}
}
