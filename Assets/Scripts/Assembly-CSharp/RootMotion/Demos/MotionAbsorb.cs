using System;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos
{
	public class MotionAbsorb : OffsetModifier
	{
		[Serializable]
		public enum Mode
		{
			Position = 0,
			PositionOffset = 1
		}

		[Serializable]
		public class Absorber
		{
			[Tooltip("The type of effector (hand, foot, shoulder...) - this is just an enum")]
			public FullBodyBipedEffector effector;

			[Tooltip("How much should motion be absorbed on this effector")]
			public float weight = 1f;

			private Vector3 position;

			private Quaternion rotation = Quaternion.identity;

			private IKEffector e;

			public void SetToBone(IKSolverFullBodyBiped solver, Mode mode)
			{
				e = solver.GetEffector(effector);
				switch (mode)
				{
				case Mode.Position:
					e.position = e.bone.position;
					e.rotation = e.bone.rotation;
					break;
				case Mode.PositionOffset:
					position = e.bone.position;
					rotation = e.bone.rotation;
					break;
				}
			}

			public void UpdateEffectorWeights(float w)
			{
				e.positionWeight = w * weight;
				e.rotationWeight = w * weight;
			}

			public void SetPosition(float w)
			{
				e.positionOffset += (position - e.bone.position) * w * weight;
			}

			public void SetRotation(float w)
			{
				e.bone.rotation = Quaternion.Slerp(e.bone.rotation, rotation, w * weight);
			}
		}

		[Tooltip("Use either effector position, position weight, rotation, rotationWeight or positionOffset and rotating the bone directly.")]
		public Mode mode;

		[Tooltip("Array containing the absorbers")]
		public Absorber[] absorbers;

		[Tooltip("Weight falloff curve (how fast will the effect reduce after impact)")]
		public AnimationCurve falloff;

		[Tooltip("How fast will the impact fade away. (if 1, effect lasts for 1 second)")]
		public float falloffSpeed = 1f;

		private float timer;

		private float w;

		private Mode initialMode;

		protected override void Start()
		{
			base.Start();
			IKSolverFullBodyBiped solver = ik.solver;
			solver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(solver.OnPostUpdate, new IKSolver.UpdateDelegate(AfterIK));
			initialMode = mode;
		}

		private void OnCollisionEnter(Collision c)
		{
			if (!(timer > 0f))
			{
				timer = 1f;
				for (int i = 0; i < absorbers.Length; i++)
				{
					absorbers[i].SetToBone(ik.solver, mode);
				}
			}
		}

		protected override void OnModifyOffset()
		{
			if (timer <= 0f)
			{
				return;
			}
			mode = initialMode;
			timer -= Time.deltaTime * falloffSpeed;
			w = falloff.Evaluate(timer);
			if (mode == Mode.Position)
			{
				for (int i = 0; i < absorbers.Length; i++)
				{
					absorbers[i].UpdateEffectorWeights(w * weight);
				}
			}
			else
			{
				for (int j = 0; j < absorbers.Length; j++)
				{
					absorbers[j].SetPosition(w * weight);
				}
			}
		}

		private void AfterIK()
		{
			if (!(timer <= 0f) && mode != 0)
			{
				for (int i = 0; i < absorbers.Length; i++)
				{
					absorbers[i].SetRotation(w * weight);
				}
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (ik != null)
			{
				IKSolverFullBodyBiped solver = ik.solver;
				solver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(solver.OnPostUpdate, new IKSolver.UpdateDelegate(AfterIK));
			}
		}
	}
}
