using UnityEngine;

namespace Klak
{
	public class BrownianMotion : MonoBehaviour
	{
		[SerializeField]
		private bool _enablePositionNoise = true;

		[SerializeField]
		private bool _enableRotationNoise = true;

		[SerializeField]
		private float _positionFrequency = 0.2f;

		[SerializeField]
		private float _rotationFrequency = 0.2f;

		[SerializeField]
		private float _positionAmplitude = 0.5f;

		[SerializeField]
		private float _rotationAmplitude = 10f;

		[SerializeField]
		private Vector3 _positionScale = Vector3.one;

		[SerializeField]
		private Vector3 _rotationScale = new Vector3(1f, 1f, 0f);

		[SerializeField]
		[Range(0f, 8f)]
		private int _positionFractalLevel = 3;

		[SerializeField]
		[Range(0f, 8f)]
		private int _rotationFractalLevel = 3;

		private const float _fbmNorm = 1.3333334f;

		private Vector3 _initialPosition;

		private Quaternion _initialRotation;

		private float[] _time;

		public bool enablePositionNoise
		{
			get
			{
				return _enablePositionNoise;
			}
			set
			{
				_enablePositionNoise = value;
			}
		}

		public bool enableRotationNoise
		{
			get
			{
				return _enableRotationNoise;
			}
			set
			{
				_enableRotationNoise = value;
			}
		}

		public float positionFrequency
		{
			get
			{
				return _positionFrequency;
			}
			set
			{
				_positionFrequency = value;
			}
		}

		public float rotationFrequency
		{
			get
			{
				return _rotationFrequency;
			}
			set
			{
				_rotationFrequency = value;
			}
		}

		public float positionAmplitude
		{
			get
			{
				return _positionAmplitude;
			}
			set
			{
				_positionAmplitude = value;
			}
		}

		public float rotationAmplitude
		{
			get
			{
				return _rotationAmplitude;
			}
			set
			{
				_rotationAmplitude = value;
			}
		}

		public Vector3 positionScale
		{
			get
			{
				return _positionScale;
			}
			set
			{
				_positionScale = value;
			}
		}

		public Vector3 rotationScale
		{
			get
			{
				return _rotationScale;
			}
			set
			{
				_rotationScale = value;
			}
		}

		public int positionFractalLevel
		{
			get
			{
				return _positionFractalLevel;
			}
			set
			{
				_positionFractalLevel = value;
			}
		}

		public int rotationFractalLevel
		{
			get
			{
				return _rotationFractalLevel;
			}
			set
			{
				_rotationFractalLevel = value;
			}
		}

		private void Start()
		{
			_time = new float[6];
			for (int i = 0; i < 6; i++)
			{
				_time[i] = Random.Range(-10000f, 0f);
			}
		}

		private void OnEnable()
		{
			_initialPosition = base.transform.localPosition;
			_initialRotation = base.transform.localRotation;
		}

		private void Update()
		{
			float deltaTime = Time.deltaTime;
			if (_enablePositionNoise)
			{
				for (int i = 0; i < 3; i++)
				{
					_time[i] += _positionFrequency * deltaTime;
				}
				Vector3 a = new Vector3(Perlin.Fbm(_time[0], _positionFractalLevel), Perlin.Fbm(_time[1], _positionFractalLevel), Perlin.Fbm(_time[2], _positionFractalLevel));
				a = Vector3.Scale(a, _positionScale);
				a *= _positionAmplitude * 1.3333334f;
				base.transform.localPosition = _initialPosition + a;
			}
			if (_enableRotationNoise)
			{
				for (int j = 0; j < 3; j++)
				{
					_time[j + 3] += _rotationFrequency * deltaTime;
				}
				Vector3 a2 = new Vector3(Perlin.Fbm(_time[3], _rotationFractalLevel), Perlin.Fbm(_time[4], _rotationFractalLevel), Perlin.Fbm(_time[5], _rotationFractalLevel));
				a2 = Vector3.Scale(a2, _rotationScale);
				a2 *= _rotationAmplitude * 1.3333334f;
				base.transform.localRotation = Quaternion.Euler(a2) * _initialRotation;
			}
		}
	}
}
