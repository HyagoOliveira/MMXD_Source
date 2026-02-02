using Unity.Jobs;

namespace MagicaCloth
{
	public abstract class PhysicsManagerWorker
	{
		public MagicaPhysicsManager Manager { get; set; }

		protected virtual void Start()
		{
		}

		public void Init(MagicaPhysicsManager manager)
		{
			Manager = manager;
			Create();
		}

		public abstract void Create();

		public abstract void RemoveGroup(int group);

		public abstract void Release();

		public abstract void Warmup();

		public abstract JobHandle PreUpdate(JobHandle jobHandle);

		public abstract JobHandle PostUpdate(JobHandle jobHandle);
	}
}
