#define RELEASE
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class cInput : MonoBehaviour
{
	public static float gravity = 3f;

	public static float sensitivity = 3f;

	public static float deadzone = 0.001f;

	private const int MAX_INPUTS = 250;

	private static bool _allowDuplicates = false;

	private static string[,] _defaultStrings = new string[250, 5];

	private static Dictionary<int, int> _inputNameHash = new Dictionary<int, int>();

	private static string[] _inputName = new string[250];

	private static KeyCode[] _inputPrimary = new KeyCode[250];

	private static KeyCode[] _modifierUsedPrimary = new KeyCode[250];

	private static KeyCode[] _inputSecondary = new KeyCode[250];

	private static KeyCode[] _modifierUsedSecondary = new KeyCode[250];

	private static List<KeyCode> _modifiers = new List<KeyCode>();

	private static List<int> _markedAsAxis = new List<int>();

	private static Dictionary<int, int> _axisNameHash = new Dictionary<int, int>();

	private static string[] _axisName = new string[250];

	private static string[] _axisPrimary = new string[250];

	private static string[] _axisSecondary = new string[250];

	private static float[] _individualAxisSens = new float[250];

	private static float[] _individualAxisGrav = new float[250];

	private static float[] _individualAxisDead = new float[250];

	private static bool[] _invertAxis = new bool[250];

	private static int[,] _makeAxis = new int[250, 2];

	private static int _inputLength = -1;

	private static int _axisLength = -1;

	private static List<KeyCode> _forbiddenKeys = new List<KeyCode>();

	private static List<string> _forbiddenAxes = new List<string>();

	private static bool[] _getKeyArray = new bool[250];

	private static bool[] _getKeyDownArray = new bool[250];

	private static bool[] _getKeyUpArray = new bool[250];

	private static bool[] _axisTriggerArrayPrimary = new bool[250];

	private static bool[] _axisTriggerArraySecondary = new bool[250];

	private static float[] _getAxis = new float[250];

	private static float[] _getAxisRaw = new float[250];

	private static float[] _getAxisArray = new float[250];

	private static float[] _getAxisArrayRaw = new float[250];

	private static int[] _tmpCalibratedAxisIndices = new int[0];

	private static bool _allowMouseAxis = false;

	private static bool _allowMouseButtons = false;

	private static bool _allowJoystickButtons = true;

	private static bool _allowJoystickAxis = true;

	private static bool _allowKeyboard = true;

	private static int _numGamepads = 4;

	private static int _numAxes = 15;

	private static bool _scanning;

	private static int _cScanIndex;

	private static int _cScanInput;

	private static cInput _cObject;

	private static bool _useGUI = true;

	private static bool _cKeysLoaded;

	private static Dictionary<string, float> _axisRawValues = new Dictionary<string, float>();

	private static string _exAllowDuplicates;

	private static string _exAxis;

	private static string _exAxisSensitivity;

	private static string _exAxisGravity;

	private static string _exAxisDeadzone;

	private static string _exAxisInverted;

	private static string _exDefaults;

	private static string _exInputs;

	private static string _exCalibrations;

	private static string _exCalibrationValues;

	private static bool _externalSaving = false;

	private static bool _usePlayerPrefs = true;

	private static Dictionary<string, KeyCode> _string2Key = new Dictionary<string, KeyCode>();

	private static int[] _axisType = new int[(_numAxes + 1) * (_numGamepads + 1)];

	private static Dictionary<string, float> _axisCalibrationOffset = new Dictionary<string, float>();

	private static string[,] _joyStrings = new string[_numGamepads + 1, _numAxes + 1];

	private static string[,] _joyStringsPos = new string[_numGamepads + 1, _numAxes + 1];

	private static string[,] _joyStringsNeg = new string[_numGamepads + 1, _numAxes + 1];

	private static Dictionary<string, int[]> _joyStringsIndices = new Dictionary<string, int[]>();

	private static Dictionary<string, int[]> _joyStringsPosIndices = new Dictionary<string, int[]>();

	private static Dictionary<string, int[]> _joyStringsNegIndices = new Dictionary<string, int[]>();

	public static bool scanning
	{
		get
		{
			return _scanning;
		}
		set
		{
			_scanning = value;
		}
	}

	public static int length
	{
		get
		{
			_cInputInit();
			return _inputLength + 1;
		}
	}

	public static bool allowDuplicates
	{
		get
		{
			_cInputInit();
			return _allowDuplicates;
		}
		set
		{
			_allowDuplicates = value;
			if (_usePlayerPrefs)
			{
				PlayerPrefs.SetString("cInput_dubl", value.ToString());
			}
			_exAllowDuplicates = value.ToString();
		}
	}

	public static bool usePlayerPrefs
	{
		get
		{
			return _usePlayerPrefs;
		}
		set
		{
			_usePlayerPrefs = value;
		}
	}

	public static bool anyKey
	{
		get
		{
			_cInputInit();
			if (Input.anyKey)
			{
				return true;
			}
			foreach (KeyValuePair<int, int> item in _inputNameHash)
			{
				if (GetKey(item.Key))
				{
					return true;
				}
			}
			return false;
		}
	}

	public static bool anyKeyDown
	{
		get
		{
			_cInputInit();
			if (Input.anyKeyDown)
			{
				return true;
			}
			foreach (KeyValuePair<int, int> item in _inputNameHash)
			{
				if (GetKeyDown(item.Key))
				{
					return true;
				}
			}
			return false;
		}
	}

	public static string externalInputs
	{
		get
		{
			return _exAllowDuplicates + "æ" + _exAxis + "æ" + _exAxisInverted + "æ" + _exDefaults + "æ" + _exInputs + "æ" + _exCalibrations + "æ" + _exCalibrationValues + "æ" + _exAxisSensitivity + "æ" + _exAxisGravity + "æ" + _exAxisDeadzone;
		}
	}

	public static event Action OnKeyChanged;

	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		for (int i = 0; i < 250; i++)
		{
			_individualAxisSens[i] = -99f;
			_individualAxisGrav[i] = -99f;
			_individualAxisDead[i] = -99f;
		}
		_CreateDictionary();
		if (!_cKeysLoaded)
		{
			if (_externalSaving)
			{
				_LoadExternalInputs();
			}
			else
			{
				_LoadInputs();
			}
		}
		AddModifier(KeyCode.None);
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (_scanning)
		{
			if (_cScanInput != 0)
			{
				_InputScans();
			}
			else
			{
				_ChangeKey(primary: (!string.IsNullOrEmpty(_axisPrimary[_cScanIndex])) ? _axisPrimary[_cScanIndex] : _inputPrimary[_cScanIndex].ToString(), secondary: (!string.IsNullOrEmpty(_axisSecondary[_cScanIndex])) ? _axisSecondary[_cScanIndex] : _inputSecondary[_cScanIndex].ToString(), num: _cScanIndex, action: _inputName[_cScanIndex]);
				_scanning = false;
				if (cInput.OnKeyChanged != null)
				{
					cInput.OnKeyChanged();
				}
			}
		}
		_CheckInputs();
	}

	public static void Init()
	{
		_cInputInit();
	}

	public static void Init(bool useGUI)
	{
		_useGUI = false;
		_cInputInit();
	}

	private static void _cInputInit()
	{
		if (_cObject == null)
		{
			GameObject gameObject = GameObject.Find("cObject");
			if (!gameObject)
			{
				gameObject = new GameObject();
				gameObject.name = "cObject";
			}
			if (gameObject.GetComponent<cInput>() == null)
			{
				_cObject = gameObject.AddComponent<cInput>();
			}
		}
	}

	private static void _CreateDictionary()
	{
		if (_string2Key.Count != 0)
		{
			return;
		}
		for (int i = 0; i <= 429; i++)
		{
			KeyCode value = (KeyCode)i;
			_string2Key.Add(value.ToString(), value);
		}
		for (int j = 0; j <= _numGamepads; j++)
		{
			for (int k = 1; k <= _numAxes; k++)
			{
				StringBuilder stringBuilder = ((j != 0) ? new StringBuilder("Joy" + j + " Axis " + k) : new StringBuilder("Joy Axis " + k));
				_joyStrings[j, k] = stringBuilder.ToString();
				_joyStringsIndices.Add(stringBuilder.ToString(), new int[2] { j, k });
				stringBuilder.Append("+");
				_joyStringsPos[j, k] = stringBuilder.ToString();
				_joyStringsPosIndices.Add(stringBuilder.ToString(), new int[2] { j, k });
				stringBuilder.Replace('+', '-');
				_joyStringsNeg[j, k] = stringBuilder.ToString();
				_joyStringsNegIndices.Add(stringBuilder.ToString(), new int[2] { j, k });
			}
		}
	}

	public static void ForbidKey(KeyCode key)
	{
		_cInputInit();
		if (!_forbiddenKeys.Contains(key) && key != 0)
		{
			_forbiddenKeys.Add(key);
		}
	}

	public static void ForbidKey(string keyString)
	{
		_cInputInit();
		ForbidKey(_String2KeyCode(keyString));
	}

	public static void ForbidAxis(string axis)
	{
		_cInputInit();
		if (_IsAxisValid(axis))
		{
			if (!_forbiddenAxes.Contains(axis))
			{
				_forbiddenAxes.Add(axis);
			}
		}
		else
		{
			Debug.LogWarning("Couldn't forbid axis " + axis + ". Not a valid axis.");
		}
	}

	public static void AddModifier(KeyCode modifierKey)
	{
		_cInputInit();
		if (!_modifiers.Contains(modifierKey))
		{
			_modifiers.Add(modifierKey);
		}
	}

	public static void AddModifier(string modifier)
	{
		_cInputInit();
		AddModifier(_String2KeyCode(modifier));
	}

	public static void RemoveModifier(KeyCode modifierKey)
	{
		_cInputInit();
		if (_modifiers.Contains(modifierKey))
		{
			_modifiers.Remove(modifierKey);
		}
	}

	public static void RemoveModifier(string modifier)
	{
		_cInputInit();
		RemoveModifier(_String2KeyCode(modifier));
	}

	public static int SetKey(string description, string primary)
	{
		return SetKey(description, primary, "None", primary, "None");
	}

	public static int SetKey(string description, string primary, string secondary)
	{
		return SetKey(description, primary, secondary, primary, secondary);
	}

	public static int SetKey(string description, string primary, string secondary, string primaryModifier)
	{
		return SetKey(description, primary, secondary, primaryModifier, secondary);
	}

	public static int SetKey(string description, string primary, string secondary, string primaryModifier, string secondaryModifier)
	{
		_cInputInit();
		if (string.IsNullOrEmpty(primaryModifier) || primaryModifier == "None")
		{
			primaryModifier = primary;
		}
		if (string.IsNullOrEmpty(secondaryModifier) || secondaryModifier == "None")
		{
			secondaryModifier = secondary;
		}
		if (_FindKeyByDescription(description) == -1)
		{
			_SetDefaultKey(_inputLength + 1, description, primary, secondary, primaryModifier, secondaryModifier);
		}
		return description.GetHashCode();
	}

	private static void _SetDefaultKey(int _num, string _name, string _input1, string _input2, string pMod, string sMod)
	{
		_defaultStrings[_num, 0] = _name;
		_defaultStrings[_num, 1] = _input1;
		_defaultStrings[_num, 2] = (string.IsNullOrEmpty(_input2) ? KeyCode.None.ToString() : _input2);
		_defaultStrings[_num, 3] = (string.IsNullOrEmpty(pMod) ? _input1 : pMod);
		_defaultStrings[_num, 4] = (string.IsNullOrEmpty(sMod) ? _input2 : sMod);
		int hashCode = _name.GetHashCode();
		if (!_inputNameHash.ContainsKey(hashCode))
		{
			_inputNameHash.Add(hashCode, _num);
		}
		if (_num > _inputLength)
		{
			_inputLength = _num;
		}
		_modifierUsedPrimary[_num] = _String2KeyCode(_defaultStrings[_num, 3]);
		_modifierUsedSecondary[_num] = _String2KeyCode(_defaultStrings[_num, 4]);
		_SetKey(_num, _name, _input1, _input2);
		_SaveDefaults();
	}

	private static void _SetKey(int _num, string _name, string _input1, string _input2)
	{
		_inputName[_num] = _name;
		_axisPrimary[_num] = "";
		if (_string2Key.Count == 0)
		{
			return;
		}
		if (!string.IsNullOrEmpty(_input1))
		{
			KeyCode keyCode = _String2KeyCode(_input1);
			_inputPrimary[_num] = keyCode;
			string text = _ChangeStringToAxisName(_input1);
			if (_input1 != text)
			{
				_axisPrimary[_num] = _input1;
			}
		}
		_axisSecondary[_num] = "";
		if (!string.IsNullOrEmpty(_input2))
		{
			KeyCode keyCode2 = _String2KeyCode(_input2);
			_inputSecondary[_num] = keyCode2;
			string text2 = _ChangeStringToAxisName(_input2);
			if (_input2 != text2)
			{
				_axisSecondary[_num] = _input2;
			}
		}
	}

	public static int SetAxis(string description, string action)
	{
		return SetAxis(description, action, "-1", sensitivity, gravity, deadzone);
	}

	public static int SetAxis(string description, string action, float axisSensitivity)
	{
		return SetAxis(description, action, "-1", axisSensitivity, gravity, deadzone);
	}

	public static int SetAxis(string description, string action, float axisSensitivity, float axisGravity)
	{
		return SetAxis(description, action, "-1", axisSensitivity, axisGravity, deadzone);
	}

	public static int SetAxis(string description, string action, float axisSensitivity, float axisGravity, float axisDeadzone)
	{
		return SetAxis(description, action, "-1", axisSensitivity, axisGravity, axisDeadzone);
	}

	public static int SetAxis(string description, string negativeInput, string positiveInput)
	{
		return SetAxis(description, negativeInput, positiveInput, sensitivity, gravity, deadzone);
	}

	public static int SetAxis(string description, string negativeInput, string positiveInput, float axisSensitivity)
	{
		return SetAxis(description, negativeInput, positiveInput, axisSensitivity, gravity, deadzone);
	}

	public static int SetAxis(string description, string negativeInput, string positiveInput, float axisSensitivity, float axisGravity)
	{
		return SetAxis(description, negativeInput, positiveInput, axisSensitivity, axisGravity, deadzone);
	}

	public static int SetAxis(string description, string negativeInput, string positiveInput, float axisSensitivity, float axisGravity, float axisDeadzone)
	{
		_cInputInit();
		if (IsKeyDefined(negativeInput))
		{
			int num = _FindAxisByDescription(description);
			if (num == -1)
			{
				num = _axisLength + 1;
			}
			int num2 = -1;
			int num3 = _FindKeyByDescription(negativeInput);
			if (IsKeyDefined(positiveInput))
			{
				num2 = _FindKeyByDescription(positiveInput);
				if (!_markedAsAxis.Contains(num2))
				{
					_markedAsAxis.Add(num2);
				}
			}
			else if (positiveInput != "-1")
			{
				Debug.LogError("Can't define Axis named: " + description + ". Please define '" + positiveInput + "' with SetKey() first.");
				return description.GetHashCode();
			}
			if (!_markedAsAxis.Contains(num3))
			{
				_markedAsAxis.Add(num3);
			}
			_SetAxis(num, description, num3, num2);
			_individualAxisSens[num3] = axisSensitivity;
			_individualAxisGrav[num3] = axisGravity;
			_individualAxisDead[num3] = axisDeadzone;
			if (num2 >= 0)
			{
				_individualAxisSens[num2] = axisSensitivity;
				_individualAxisGrav[num2] = axisGravity;
				_individualAxisDead[num2] = axisDeadzone;
			}
		}
		else
		{
			Debug.LogError("Can't define Axis named: " + description + ". Please define '" + negativeInput + "' with SetKey() first.");
		}
		return description.GetHashCode();
	}

	private static void _SetAxis(int _num, string _description, int _negative, int _positive)
	{
		if (_num > _axisLength)
		{
			_axisLength = _num;
		}
		int hashCode = _description.GetHashCode();
		if (!_axisNameHash.ContainsKey(hashCode))
		{
			_axisNameHash.Add(hashCode, _num);
		}
		_axisName[_num] = _description;
		_makeAxis[_num, 0] = _negative;
		_makeAxis[_num, 1] = _positive;
		_SaveAxis();
	}

	public static void SetAxisSensitivity(string axisName, float sensitivity)
	{
		_SetAxisSensitivity(axisName.GetHashCode(), sensitivity, axisName);
	}

	public static void SetAxisSensitivity(int axisHash, float sensitivity)
	{
		_SetAxisSensitivity(axisHash, sensitivity);
	}

	public static void SetAxisGravity(string axisName, float gravity)
	{
		_SetAxisGravity(axisName.GetHashCode(), gravity, axisName);
	}

	public static void SetAxisGravity(int axisHash, float gravity)
	{
		_SetAxisGravity(axisHash, gravity);
	}

	public static void SetAxisDeadzone(string axisName, float deadzone)
	{
		_SetAxisDeadzone(axisName.GetHashCode(), deadzone, axisName);
	}

	public static void SetAxisDeadzone(int axisHash, float deadzone)
	{
		_SetAxisDeadzone(axisHash, deadzone);
	}

	private static void _SetAxisSensitivity(int hash, float sensitivity, string description = "")
	{
		_cInputInit();
		int num = _FindAxisByHash(hash);
		if (num == -1)
		{
			string text = ((!string.IsNullOrEmpty(description)) ? description : ("axis matching hashcode of " + hash));
			Debug.LogError("Cannot set sensitivity of " + text + ". Have you defined this axis with SetAxis() yet?");
			return;
		}
		_individualAxisSens[_makeAxis[num, 0]] = sensitivity;
		if (_makeAxis[num, 1] >= 0 && _FindKeyByHash(_inputName[_makeAxis[num, 1]].GetHashCode()) >= 0)
		{
			_individualAxisSens[_makeAxis[num, 1]] = sensitivity;
		}
		_SaveAxisSensitivity();
	}

	private static void _SetAxisGravity(int hash, float gravity, string description = "")
	{
		_cInputInit();
		int num = _FindAxisByHash(hash);
		if (num == -1)
		{
			string text = ((!string.IsNullOrEmpty(description)) ? description : ("axis matching hashcode of " + hash));
			Debug.LogError("Cannot set gravity of " + text + ". Have you defined this axis with SetAxis() yet?");
			return;
		}
		_individualAxisGrav[_makeAxis[num, 0]] = gravity;
		if (_makeAxis[num, 1] >= 0 && _FindKeyByHash(_inputName[_makeAxis[num, 1]].GetHashCode()) >= 0)
		{
			_individualAxisGrav[_makeAxis[num, 1]] = gravity;
		}
		_SaveAxisGravity();
	}

	private static void _SetAxisDeadzone(int hash, float deadzone, string description = "")
	{
		_cInputInit();
		int num = _FindAxisByHash(hash);
		if (num == -1)
		{
			string text = ((!string.IsNullOrEmpty(description)) ? description : ("axis matching hashcode of " + hash));
			Debug.LogError("Cannot set deadzone of " + text + ". Have you defined this axis with SetAxis() yet?");
			return;
		}
		_individualAxisDead[_makeAxis[num, 0]] = deadzone;
		if (_makeAxis[num, 1] >= 0 && _FindKeyByHash(_inputName[_makeAxis[num, 1]].GetHashCode()) >= 0)
		{
			_individualAxisDead[_makeAxis[num, 1]] = deadzone;
		}
		_SaveAxisDeadzone();
	}

	private static int _FindKeyByHash(int hash)
	{
		if (!_inputNameHash.ContainsKey(hash))
		{
			return -1;
		}
		return _inputNameHash[hash];
	}

	private static int _FindKeyByDescription(string description)
	{
		return _FindKeyByHash(description.GetHashCode());
	}

	private static bool _GetKey(int hash, string description = "")
	{
		_cInputInit();
		if (!_DefaultsExist())
		{
			Debug.LogError("No default inputs found. Please setup your default inputs with SetKey first.");
			return false;
		}
		if (!_cKeysLoaded)
		{
			return false;
		}
		int num = _FindKeyByHash(hash);
		if (num > -1)
		{
			return _getKeyArray[num];
		}
		string text = ((!string.IsNullOrEmpty(description)) ? description : ("hash " + hash));
		Debug.LogError("Couldn't find a key match for " + text + ". Is it possible you typed it wrong or forgot to setup your defaults after making changes?");
		return false;
	}

	public static bool GetKey(string description)
	{
		return _GetKey(description.GetHashCode(), description);
	}

	public static bool GetKey(int descriptionHash)
	{
		return _GetKey(descriptionHash);
	}

	private static bool _GetKeyDown(int hash, string description = "")
	{
		_cInputInit();
		if (!_DefaultsExist())
		{
			Debug.LogError("No default inputs found. Please setup your default inputs with SetKey first.");
			return false;
		}
		if (!_cKeysLoaded)
		{
			return false;
		}
		int num = _FindKeyByHash(hash);
		if (num > -1)
		{
			return _getKeyDownArray[num];
		}
		string text = ((!string.IsNullOrEmpty(description)) ? description : ("hash " + hash));
		Debug.LogError("Couldn't find a key match for " + text + ". Is it possible you typed it wrong or forgot to setup your defaults after making changes?");
		return false;
	}

	public static bool GetKeyDown(string description)
	{
		return _GetKeyDown(description.GetHashCode(), description);
	}

	public static bool GetKeyDown(int descriptionHash)
	{
		return _GetKeyDown(descriptionHash);
	}

	private static bool _GetKeyUp(int hash, string description = "")
	{
		_cInputInit();
		if (!_DefaultsExist())
		{
			Debug.LogError("No default inputs found. Please setup your default inputs with SetKey first.");
			return false;
		}
		if (!_cKeysLoaded)
		{
			return false;
		}
		int num = _FindKeyByHash(hash);
		if (num > -1)
		{
			return _getKeyUpArray[num];
		}
		string text = ((!string.IsNullOrEmpty(description)) ? description : ("hash " + hash));
		Debug.LogError("Couldn't find a key match for " + text + ". Is it possible you typed it wrong or forgot to setup your defaults after making changes?");
		return false;
	}

	public static bool GetKeyUp(string description)
	{
		return _GetKeyUp(description.GetHashCode(), description);
	}

	public static bool GetKeyUp(int descriptionHash)
	{
		return _GetKeyUp(descriptionHash);
	}

	public static bool GetButton(string description)
	{
		return GetKey(description);
	}

	public static bool GetButton(int descriptionHash)
	{
		return GetKey(descriptionHash);
	}

	public static bool GetButtonDown(string description)
	{
		return GetKeyDown(description);
	}

	public static bool GetButtonDown(int descriptionHash)
	{
		return GetKeyDown(descriptionHash);
	}

	public static bool GetButtonUp(string description)
	{
		return GetKeyUp(description);
	}

	public static bool GetButtonUp(int descriptionHash)
	{
		return GetKeyUp(descriptionHash);
	}

	private static int _FindAxisByHash(int hash)
	{
		if (!_axisNameHash.ContainsKey(hash))
		{
			return -1;
		}
		return _axisNameHash[hash];
	}

	private static int _FindAxisByDescription(string description)
	{
		return _FindAxisByHash(description.GetHashCode());
	}

	private static float _GetAxis(int hash, string description = "")
	{
		_cInputInit();
		if (!_DefaultsExist())
		{
			Debug.LogError("No default inputs found. Please setup your default inputs with SetKey first.");
			return 0f;
		}
		int num = _FindAxisByHash(hash);
		if (num > -1)
		{
			if (_invertAxis[num])
			{
				return _getAxisArray[num] * -1f;
			}
			return _getAxisArray[num];
		}
		string text = ((!string.IsNullOrEmpty(description)) ? description : ("axis with hashcode of " + hash));
		Debug.LogError("Couldn't find an axis match for " + text + ". Is it possible you typed it wrong?");
		return 0f;
	}

	public static float GetAxis(string description)
	{
		return _GetAxis(description.GetHashCode(), description);
	}

	public static float GetAxis(int descriptionHash)
	{
		return _GetAxis(descriptionHash);
	}

	private static float _GetAxisRaw(int hash, string description = "")
	{
		_cInputInit();
		if (!_DefaultsExist())
		{
			Debug.LogError("No default inputs found. Please setup your default inputs with SetKey first.");
			return 0f;
		}
		int num = _FindAxisByHash(hash);
		if (num > -1)
		{
			if (_invertAxis[num])
			{
				return _getAxisArrayRaw[num] * -1f;
			}
			return _getAxisArrayRaw[num];
		}
		string text = ((!string.IsNullOrEmpty(description)) ? description : ("axis with hashcode of " + hash));
		Debug.LogError("Couldn't find an axis match for " + text + ". Is it possible you typed it wrong?");
		return 0f;
	}

	public static float GetAxisRaw(string description)
	{
		return _GetAxisRaw(description.GetHashCode(), description);
	}

	public static float GetAxisRaw(int descriptionHash)
	{
		return _GetAxisRaw(descriptionHash);
	}

	private static float _GetAxisSensitivity(int hash, string description = "")
	{
		_cInputInit();
		int num = _FindAxisByHash(hash);
		if (num == -1)
		{
			string text = ((!string.IsNullOrEmpty(description)) ? description : ("axis with hashcode of " + hash));
			Debug.LogError("Cannot get sensitivity of " + text + ". Have you defined this axis with SetAxis() yet?");
			return -1f;
		}
		return _individualAxisSens[_makeAxis[num, 0]];
	}

	public static float GetAxisSensitivity(string description)
	{
		return _GetAxisSensitivity(description.GetHashCode(), description);
	}

	public static float GetAxisSensitivity(int descriptionHash)
	{
		return _GetAxisSensitivity(descriptionHash);
	}

	private static float _GetAxisGravity(int hash, string description = "")
	{
		_cInputInit();
		int num = _FindAxisByHash(hash);
		if (num == -1)
		{
			string text = ((!string.IsNullOrEmpty(description)) ? description : ("axis with hashcode of " + hash));
			Debug.LogError("Cannot get gravity of " + text + ". Have you defined this axis with SetAxis() yet?");
			return -1f;
		}
		return _individualAxisGrav[_makeAxis[num, 0]];
	}

	public static float GetAxisGravity(string description)
	{
		return _GetAxisGravity(description.GetHashCode(), description);
	}

	public static float GetAxisGravity(int descriptionHash)
	{
		return _GetAxisGravity(descriptionHash);
	}

	private static float _GetAxisDeadzone(int hash, string description = "")
	{
		_cInputInit();
		int num = _FindAxisByHash(hash);
		if (num == -1)
		{
			string text = ((!string.IsNullOrEmpty(description)) ? description : ("axis with hashcode of " + hash));
			Debug.LogError("Cannot get deadzone of " + text + ". Have you defined this axis with SetAxis() yet?");
			return -1f;
		}
		return _individualAxisDead[_makeAxis[num, 0]];
	}

	public static float GetAxisDeadzone(string description)
	{
		return _GetAxisDeadzone(description.GetHashCode(), description);
	}

	public static float GetAxisDeadzone(int descriptionHash)
	{
		return _GetAxisDeadzone(descriptionHash);
	}

	public static string GetText(string action)
	{
		return GetText(action, 1);
	}

	public static string GetText(int index)
	{
		return GetText(index, 0);
	}

	public static string GetText(string action, int input)
	{
		int num = _FindKeyByDescription(action);
		if (num == -1)
		{
			Debug.LogWarning("Could not get text for action " + action + ". It doesn't exist.");
			return "Invalid Input";
		}
		return GetText(num, input);
	}

	public static string GetText(string action, int input, bool returnBlank)
	{
		string text = GetText(action, input);
		if (returnBlank && text == "None")
		{
			text = "";
		}
		return text;
	}

	public static string GetText(int index, int input, bool returnBlank)
	{
		string text = GetText(index, input);
		if (returnBlank && text == "None")
		{
			text = "";
		}
		return text;
	}

	public static string GetText(int index, int input)
	{
		_cInputInit();
		if (input < 0 || input > 2)
		{
			Debug.LogWarning("Can't look for text #" + input + " for " + _inputName[index] + " input. Only 0, 1, or 2 is acceptable. Clamping to this range.");
			input = Mathf.Clamp(input, 0, 2);
		}
		if (index < 0 || index >= _inputName.Length || index >= _axisPrimary.Length || index >= _axisSecondary.Length)
		{
			Debug.LogWarning("Index out of range when calling GetText for input at index " + index + ". Are you sure you've set up cInput properly?");
			return "Invalid Input";
		}
		if (index >= length)
		{
			Debug.LogWarning("Index out of range when calling GetText for input at index " + index + ". Index should be less than " + length + ".");
			return "Invalid Input";
		}
		string result;
		switch (input)
		{
		case 1:
		{
			if (!string.IsNullOrEmpty(_axisPrimary[index]))
			{
				result = _axisPrimary[index];
				break;
			}
			string text2 = "";
			if (_modifierUsedPrimary[index] != 0 && _modifierUsedPrimary[index] != _inputPrimary[index] && _inputPrimary[index] != 0)
			{
				text2 = _modifierUsedPrimary[index].ToString() + " + ";
			}
			result = text2 + _inputPrimary[index];
			break;
		}
		case 2:
		{
			if (!string.IsNullOrEmpty(_axisSecondary[index]))
			{
				result = _axisSecondary[index];
				break;
			}
			string text = "";
			if (_modifierUsedSecondary[index] != 0 && _modifierUsedSecondary[index] != _inputSecondary[index] && _inputSecondary[index] != 0)
			{
				text = _modifierUsedSecondary[index].ToString() + " + ";
			}
			result = text + _inputSecondary[index];
			break;
		}
		default:
			return _inputName[index];
		}
		if (_scanning && index == _cScanIndex && input == _cScanInput)
		{
			result = ". . .";
		}
		return result;
	}

	private static string _ChangeStringToAxisName(string description)
	{
		switch (description)
		{
		case "Mouse Left":
			return "Mouse Horizontal";
		case "Mouse Right":
			return "Mouse Horizontal";
		case "Mouse Up":
			return "Mouse Vertical";
		case "Mouse Down":
			return "Mouse Vertical";
		case "Mouse Wheel Up":
			return "Mouse Wheel";
		case "Mouse Wheel Down":
			return "Mouse Wheel";
		default:
		{
			string text = _FindJoystringByDescription(description);
			if (!string.IsNullOrEmpty(text))
			{
				return text;
			}
			return description;
		}
		}
	}

	private static string _FindJoystringByDescription(string description)
	{
		if (_joyStringsPosIndices.ContainsKey(description))
		{
			int[] array = _joyStringsPosIndices[description];
			return _joyStrings[array[0], array[1]];
		}
		if (_joyStringsNegIndices.ContainsKey(description))
		{
			int[] array = _joyStringsNegIndices[description];
			return _joyStrings[array[0], array[1]];
		}
		return null;
	}

	private static bool _IsAxisValid(string axis)
	{
		switch (axis)
		{
		case "Mouse Left":
		case "Mouse Right":
		case "Mouse Up":
		case "Mouse Down":
		case "Mouse Wheel Up":
		case "Mouse Wheel Down":
			return true;
		default:
			if (!_joyStringsPosIndices.ContainsKey(axis))
			{
				return _joyStringsNegIndices.ContainsKey(axis);
			}
			return true;
		}
	}

	private static int _PosOrNeg(string description)
	{
		int result = 1;
		switch (description)
		{
		case "Mouse Left":
			return -1;
		case "Mouse Down":
			return -1;
		case "Mouse Wheel Down":
			return -1;
		default:
			if (_joyStringsNegIndices.ContainsKey(description))
			{
				return -1;
			}
			return result;
		}
	}

	public static void ChangeKey(string action, int input, bool allowMouseAxis, bool allowMouseButtons, bool allowGamepadAxis, bool allowGamepadButtons, bool allowKeyboard)
	{
		_cInputInit();
		int num = _FindKeyByDescription(action);
		if (num < 0)
		{
			Debug.LogError("Trying to change cInput action named " + action + ", but couldn't find it. Did you create it first with SetKey()?");
		}
		else
		{
			ChangeKey(num, input, allowMouseAxis, allowMouseButtons, allowGamepadAxis, allowGamepadButtons, allowKeyboard);
		}
	}

	public static void ChangeKey(string action)
	{
		ChangeKey(action, 1, _allowMouseAxis, _allowMouseButtons, _allowJoystickAxis, _allowJoystickButtons, _allowKeyboard);
	}

	public static void ChangeKey(string action, int input)
	{
		ChangeKey(action, input, _allowMouseAxis, _allowMouseButtons, _allowJoystickAxis, _allowJoystickButtons, _allowKeyboard);
	}

	public static void ChangeKey(string action, int input, bool allowMouseAxis)
	{
		ChangeKey(action, input, allowMouseAxis, _allowMouseButtons, _allowJoystickAxis, _allowJoystickButtons, _allowKeyboard);
	}

	public static void ChangeKey(string action, int input, bool allowMouseAxis, bool allowMouseButtons)
	{
		ChangeKey(action, input, allowMouseAxis, allowMouseButtons, _allowJoystickAxis, _allowJoystickButtons, _allowKeyboard);
	}

	public static void ChangeKey(string action, int input, bool allowMouseAxis, bool allowMouseButtons, bool allowGamepadAxis)
	{
		ChangeKey(action, input, allowMouseAxis, allowMouseButtons, allowGamepadAxis, _allowJoystickButtons, _allowKeyboard);
	}

	public static void ChangeKey(string action, int input, bool allowMouseAxis, bool allowMouseButtons, bool allowGamepadAxis, bool allowGamepadButtons)
	{
		ChangeKey(action, input, allowMouseAxis, allowMouseButtons, allowGamepadAxis, allowGamepadButtons, _allowKeyboard);
	}

	public static void ChangeKey(int index, int input, bool allowMouseAxis, bool allowMouseButtons, bool allowGamepadAxis, bool allowGamepadButtons, bool allowKeyboard)
	{
		_cInputInit();
		if (input != 1 && input != 2)
		{
			Debug.LogWarning("ChangeKey can only change primary (1) or secondary (2) inputs. You're trying to change: " + input);
		}
		else
		{
			_ScanForNewKey(index, input, allowMouseAxis, allowMouseButtons, allowGamepadAxis, allowGamepadButtons, allowKeyboard);
		}
	}

	public static void ChangeKey(int index)
	{
		ChangeKey(index, 1, _allowMouseAxis, _allowMouseButtons, _allowJoystickAxis, _allowJoystickButtons, _allowKeyboard);
	}

	public static void ChangeKey(int index, int input)
	{
		ChangeKey(index, input, _allowMouseAxis, _allowMouseButtons, _allowJoystickAxis, _allowJoystickButtons, _allowKeyboard);
	}

	public static void ChangeKey(int index, int input, bool allowMouseAxis)
	{
		ChangeKey(index, input, allowMouseAxis, _allowMouseButtons, _allowJoystickAxis, _allowJoystickButtons, _allowKeyboard);
	}

	public static void ChangeKey(int index, int input, bool allowMouseAxis, bool allowMouseButtons)
	{
		ChangeKey(index, input, allowMouseAxis, allowMouseButtons, _allowJoystickAxis, _allowJoystickButtons, _allowKeyboard);
	}

	public static void ChangeKey(int index, int input, bool allowMouseAxis, bool allowMouseButtons, bool allowGamepadAxis)
	{
		ChangeKey(index, input, allowMouseAxis, allowMouseButtons, allowGamepadAxis, _allowJoystickButtons, _allowKeyboard);
	}

	public static void ChangeKey(int index, int input, bool allowMouseAxis, bool allowMouseButtons, bool allowGamepadAxis, bool allowGamepadButtons)
	{
		ChangeKey(index, input, allowMouseAxis, allowMouseButtons, allowGamepadAxis, allowGamepadButtons, _allowKeyboard);
	}

	public static void ChangeKey(string action, string primary, string secondary, string primaryModifier, string secondaryModifier)
	{
		_cInputInit();
		int num = _FindKeyByDescription(action);
		if (string.IsNullOrEmpty(primaryModifier))
		{
			primaryModifier = primary;
		}
		if (string.IsNullOrEmpty(secondaryModifier))
		{
			secondaryModifier = secondary;
		}
		_modifierUsedPrimary[num] = _String2KeyCode(primaryModifier);
		_modifierUsedSecondary[num] = _String2KeyCode(secondaryModifier);
		_ChangeKey(num, action, primary, secondary);
	}

	public static void ChangeKey(string action, string primary)
	{
		int num = _FindKeyByDescription(action);
		ChangeKey(action, primary, "", primary, _modifierUsedSecondary[num].ToString());
	}

	public static void ChangeKey(string action, string primary, string secondary)
	{
		ChangeKey(action, primary, secondary, primary, secondary);
	}

	public static void ChangeKey(string action, string primary, string secondary, string primaryModifier)
	{
		ChangeKey(action, primary, secondary, primaryModifier, secondary);
	}

	private static void _ScanForNewKey(int index, int input, bool mouseAx, bool mouseBut, bool joyAx, bool joyBut, bool keyb)
	{
		_allowMouseAxis = mouseAx;
		_allowMouseButtons = mouseBut;
		_allowJoystickButtons = joyBut;
		_allowJoystickAxis = joyAx;
		_allowKeyboard = keyb;
		_cScanInput = input;
		_cScanIndex = index;
		_scanning = true;
		_axisRawValues = _GetAxisRawValues();
	}

	private static Dictionary<string, float> _GetAxisRawValues()
	{
		Dictionary<string, float> dictionary = new Dictionary<string, float>();
		dictionary.Add("Horizontal", Input.GetAxisRaw("Horizontal"));
		dictionary.Add("Vertical", Input.GetAxisRaw("Vertical"));
		dictionary.Add("Mouse X", Input.GetAxisRaw("Mouse X"));
		dictionary.Add("Mouse Y", Input.GetAxisRaw("Mouse Y"));
		dictionary.Add("Mouse Horizontal", Input.GetAxisRaw("Mouse Horizontal"));
		dictionary.Add("Mouse Vertical", Input.GetAxisRaw("Mouse Vertical"));
		dictionary.Add("Mouse ScrollWheel", Input.GetAxisRaw("Mouse ScrollWheel"));
		dictionary.Add("Mouse Wheel", Input.GetAxisRaw("Mouse Wheel"));
		dictionary.Add("Window Shake X", Input.GetAxisRaw("Window Shake X"));
		dictionary.Add("Window Shake Y", Input.GetAxisRaw("Window Shake Y"));
		string text = "";
		for (int i = 1; i <= _numGamepads; i++)
		{
			for (int j = 1; j <= _numAxes; j++)
			{
				text = "Joy" + i + " Axis " + j;
				dictionary.Add(text, Input.GetAxis(text));
			}
		}
		return dictionary;
	}

	private static void _ChangeKey(int num, string action, string primary, string secondary)
	{
		_SetKey(num, action, primary, secondary);
		_SaveInputs();
	}

	private static void _SaveAxis()
	{
		int num = _axisLength + 1;
		string text = "";
		string text2 = "";
		string text3 = "";
		for (int i = 0; i < num; i++)
		{
			text = text + _axisName[i] + "*";
			text2 = text2 + _makeAxis[i, 0] + "*";
			text3 = text3 + _makeAxis[i, 1] + "*";
		}
		string text4 = text + "#" + text2 + "#" + text3 + "#" + num;
		if (_usePlayerPrefs)
		{
			PlayerPrefs.SetString("cInput_axis", text4);
		}
		_exAxis = text4;
	}

	private static void _SaveAxisSensitivity()
	{
		int num = _inputLength + 1;
		string text = "";
		for (int i = 0; i < num; i++)
		{
			text = text + _individualAxisSens[i] + "*";
		}
		if (_usePlayerPrefs)
		{
			PlayerPrefs.SetString("cInput_indAxSens", text);
		}
		_exAxisSensitivity = text;
	}

	private static void _SaveAxisGravity()
	{
		int num = _inputLength + 1;
		string text = "";
		for (int i = 0; i < num; i++)
		{
			text = text + _individualAxisGrav[i] + "*";
		}
		if (_usePlayerPrefs)
		{
			PlayerPrefs.SetString("cInput_indAxGrav", text);
		}
		_exAxisGravity = text;
	}

	private static void _SaveAxisDeadzone()
	{
		int num = _inputLength + 1;
		string text = "";
		for (int i = 0; i < num; i++)
		{
			text = text + _individualAxisDead[i] + "*";
		}
		if (_usePlayerPrefs)
		{
			PlayerPrefs.SetString("cInput_indAxDead", text);
		}
		_exAxisDeadzone = text;
	}

	private static void _SaveAxInverted()
	{
		int num = _axisLength + 1;
		string text = "";
		for (int i = 0; i < num; i++)
		{
			text = text + _invertAxis[i] + "*";
		}
		if (_usePlayerPrefs)
		{
			PlayerPrefs.SetString("cInput_axInv", text);
		}
		_exAxisInverted = text;
	}

	private static void _SaveDefaults()
	{
		int num = _inputLength + 1;
		string text = "";
		string text2 = "";
		string text3 = "";
		string text4 = "";
		string text5 = "";
		for (int i = 0; i < num; i++)
		{
			text = text + _defaultStrings[i, 0] + "*";
			text2 = text2 + _defaultStrings[i, 1] + "*";
			text3 = text3 + _defaultStrings[i, 2] + "*";
			text4 = text4 + _defaultStrings[i, 3] + "*";
			text5 = text5 + _defaultStrings[i, 4] + "*";
		}
		string text6 = text + "#" + text2 + "#" + text3 + "#" + text4 + "#" + text5;
		if (_usePlayerPrefs)
		{
			PlayerPrefs.SetInt("cInput_count", num);
			PlayerPrefs.SetString("cInput_defaults", text6);
		}
		_exDefaults = num + "¿" + text6;
	}

	private static void _SaveInputs()
	{
		int num = _inputLength + 1;
		string text = "";
		string text2 = "";
		string text3 = "";
		string text4 = "";
		string text5 = "";
		string text6 = "";
		string text7 = "";
		for (int i = 0; i < num; i++)
		{
			text = text + _inputName[i] + "*";
			text2 = string.Concat(text2, _inputPrimary[i], "*");
			text3 = string.Concat(text3, _inputSecondary[i], "*");
			text4 = text4 + _axisPrimary[i] + "*";
			text5 = text5 + _axisSecondary[i] + "*";
			text6 = string.Concat(text6, _modifierUsedPrimary[i], "*");
			text7 = string.Concat(text7, _modifierUsedSecondary[i], "*");
		}
		if (_usePlayerPrefs)
		{
			PlayerPrefs.SetString("cInput_descr", text);
			PlayerPrefs.SetString("cInput_inp", text2);
			PlayerPrefs.SetString("cInput_alt_inp", text3);
			PlayerPrefs.SetString("cInput_inpStr", text4);
			PlayerPrefs.SetString("cInput_alt_inpStr", text5);
			PlayerPrefs.SetString("cInput_modifierStr", text6);
			PlayerPrefs.SetString("cInput_alt_modifierStr", text7);
		}
		_exInputs = text + "¿" + text2 + "¿" + text3 + "¿" + text4 + "¿" + text5 + "¿" + text6 + "¿" + text7;
	}

	public static void LoadExternal(string externString)
	{
		_cInputInit();
		string[] array = externString.Split('æ');
		_exAllowDuplicates = array[0];
		_exAxis = array[1];
		_exAxisInverted = array[2];
		_exDefaults = array[3];
		_exInputs = array[4];
		_exCalibrations = array[5];
		_exCalibrationValues = array[6];
		if (array.Length > 7)
		{
			_exAxisSensitivity = array[7];
			_exAxisGravity = array[8];
			_exAxisDeadzone = array[9];
		}
		_LoadExternalInputs();
	}

	private static void _LoadInputs()
	{
		if (!_usePlayerPrefs || !PlayerPrefs.HasKey("cInput_count"))
		{
			_cKeysLoaded = true;
			return;
		}
		if (PlayerPrefs.HasKey("cInput_dubl"))
		{
			if (PlayerPrefs.GetString("cInput_dubl") == "True")
			{
				allowDuplicates = true;
			}
			else
			{
				allowDuplicates = false;
			}
		}
		_inputLength = PlayerPrefs.GetInt("cInput_count") - 1;
		string[] array = PlayerPrefs.GetString("cInput_defaults").Split('#');
		string[] array2 = array[0].Split('*');
		string[] array3 = array[1].Split('*');
		string[] array4 = array[2].Split('*');
		string[] array5 = array[3].Split('*');
		string[] array6 = array[4].Split('*');
		for (int i = 0; i < array2.Length - 1; i++)
		{
			_SetDefaultKey(i, array2[i], array3[i], array4[i], array5[i], array6[i]);
		}
		if (PlayerPrefs.HasKey("cInput_inp"))
		{
			string @string = PlayerPrefs.GetString("cInput_descr");
			string string2 = PlayerPrefs.GetString("cInput_inp");
			string string3 = PlayerPrefs.GetString("cInput_alt_inp");
			string string4 = PlayerPrefs.GetString("cInput_inpStr");
			string string5 = PlayerPrefs.GetString("cInput_alt_inpStr");
			string string6 = PlayerPrefs.GetString("cInput_modifierStr");
			string string7 = PlayerPrefs.GetString("cInput_alt_modifierStr");
			string[] array7 = @string.Split('*');
			string[] array8 = string2.Split('*');
			string[] array9 = string3.Split('*');
			string[] array10 = string4.Split('*');
			string[] array11 = string5.Split('*');
			string[] array12 = string6.Split('*');
			string[] array13 = string7.Split('*');
			for (int j = 0; j < array7.Length - 1; j++)
			{
				if (array7[j] == _defaultStrings[j, 0])
				{
					_inputName[j] = array7[j];
					_inputPrimary[j] = _String2KeyCode(array8[j]);
					_inputSecondary[j] = _String2KeyCode(array9[j]);
					_axisPrimary[j] = array10[j];
					_axisSecondary[j] = array11[j];
					_modifierUsedPrimary[j] = _String2KeyCode(array12[j]);
					_modifierUsedSecondary[j] = _String2KeyCode(array13[j]);
				}
			}
			for (int k = 0; k < array2.Length - 1; k++)
			{
				for (int l = 0; l < array7.Length - 1; l++)
				{
					if (array7[l] == _defaultStrings[k, 0])
					{
						_inputName[k] = array7[l];
						_inputPrimary[k] = _String2KeyCode(array8[l]);
						_inputSecondary[k] = _String2KeyCode(array9[l]);
						_axisPrimary[k] = array10[l];
						_axisSecondary[k] = array11[l];
						_modifierUsedPrimary[l] = _String2KeyCode(array12[l]);
						_modifierUsedSecondary[l] = _String2KeyCode(array13[l]);
					}
				}
			}
		}
		if (PlayerPrefs.HasKey("cInput_axis"))
		{
			string[] array14 = PlayerPrefs.GetString("cInput_axis").Split('#');
			string[] array15 = array14[0].Split('*');
			string[] array16 = array14[1].Split('*');
			string[] array17 = array14[2].Split('*');
			int num = int.Parse(array14[3]);
			for (int m = 0; m < num; m++)
			{
				int negative = int.Parse(array16[m]);
				int positive = int.Parse(array17[m]);
				_SetAxis(m, array15[m], negative, positive);
			}
		}
		if (PlayerPrefs.HasKey("cInput_axInv"))
		{
			string[] array18 = PlayerPrefs.GetString("cInput_axInv").Split('*');
			for (int n = 0; n < array18.Length; n++)
			{
				if (array18[n] == "True")
				{
					_invertAxis[n] = true;
				}
				else
				{
					_invertAxis[n] = false;
				}
			}
		}
		if (PlayerPrefs.HasKey("cInput_indAxSens"))
		{
			string[] array19 = PlayerPrefs.GetString("cInput_indAxSens").Split('*');
			for (int num2 = 0; num2 < array19.Length - 1; num2++)
			{
				_individualAxisSens[num2] = float.Parse(array19[num2]);
			}
		}
		if (PlayerPrefs.HasKey("cInput_indAxGrav"))
		{
			string[] array20 = PlayerPrefs.GetString("cInput_indAxGrav").Split('*');
			for (int num3 = 0; num3 < array20.Length - 1; num3++)
			{
				_individualAxisGrav[num3] = float.Parse(array20[num3]);
			}
		}
		if (PlayerPrefs.HasKey("cInput_indAxDead"))
		{
			string[] array21 = PlayerPrefs.GetString("cInput_indAxDead").Split('*');
			for (int num4 = 0; num4 < array21.Length - 1; num4++)
			{
				_individualAxisDead[num4] = float.Parse(array21[num4]);
			}
		}
		if (PlayerPrefs.HasKey("cInput_saveCals"))
		{
			string[] array22 = PlayerPrefs.GetString("cInput_saveCals").Split('*');
			for (int num5 = 0; num5 < array22.Length - 1; num5++)
			{
				_axisType[num5] = int.Parse(array22[num5]);
			}
		}
		if (PlayerPrefs.HasKey("cInput_calsVals"))
		{
			_CalibrationValuesFromString(PlayerPrefs.GetString("cInput_calsVals"));
		}
		_cKeysLoaded = true;
	}

	private static void _LoadExternalInputs()
	{
		_externalSaving = true;
		string[] array = _exDefaults.Split('¿');
		string[] array2 = _exInputs.Split('¿');
		allowDuplicates = _exAllowDuplicates == "True";
		_inputLength = int.Parse(array[0]) - 1;
		string[] array3 = array[1].Split('#');
		string[] array4 = array3[0].Split('*');
		string[] array5 = array3[1].Split('*');
		string[] array6 = array3[2].Split('*');
		string[] array7 = array3[3].Split('*');
		string[] array8 = array3[4].Split('*');
		for (int i = 0; i < array4.Length - 1; i++)
		{
			_SetDefaultKey(i, array4[i], array5[i], array6[i], array7[i], array8[i]);
		}
		if (!string.IsNullOrEmpty(array2[0]))
		{
			string text = array2[0];
			string text2 = array2[1];
			string text3 = array2[2];
			string text4 = array2[3];
			string text5 = array2[4];
			string text6 = array2[5];
			string text7 = array2[6];
			string[] array9 = text.Split('*');
			string[] array10 = text2.Split('*');
			string[] array11 = text3.Split('*');
			string[] array12 = text4.Split('*');
			string[] array13 = text5.Split('*');
			string[] array14 = text6.Split('*');
			string[] array15 = text7.Split('*');
			for (int j = 0; j < array9.Length - 1; j++)
			{
				if (array9[j] == _defaultStrings[j, 0])
				{
					_inputName[j] = array9[j];
					_inputPrimary[j] = _String2KeyCode(array10[j]);
					_inputSecondary[j] = _String2KeyCode(array11[j]);
					_axisPrimary[j] = array12[j];
					_axisSecondary[j] = array13[j];
					_modifierUsedPrimary[j] = _String2KeyCode(array14[j]);
					_modifierUsedSecondary[j] = _String2KeyCode(array15[j]);
				}
			}
			for (int k = 0; k < array4.Length - 1; k++)
			{
				for (int l = 0; l < array9.Length - 1; l++)
				{
					if (array9[l] == _defaultStrings[k, 0])
					{
						_inputName[k] = array9[l];
						_inputPrimary[k] = _String2KeyCode(array10[l]);
						_inputSecondary[k] = _String2KeyCode(array11[l]);
						_axisPrimary[k] = array12[l];
						_axisSecondary[k] = array13[l];
						_modifierUsedPrimary[l] = _String2KeyCode(array14[l]);
						_modifierUsedSecondary[l] = _String2KeyCode(array15[l]);
					}
				}
			}
		}
		string[] array16 = _exAxis.Split('¿');
		if (array16.Length > 1)
		{
			if (!string.IsNullOrEmpty(array16[0]))
			{
				string exAxisInverted = _exAxisInverted;
				string[] array17 = ((!string.IsNullOrEmpty(exAxisInverted)) ? exAxisInverted.Split('*') : null);
				string[] array18 = array16[0].Split('#');
				string[] array19 = array18[0].Split('*');
				string[] array20 = array18[1].Split('*');
				string[] array21 = array18[2].Split('*');
				int num = int.Parse(array18[3]);
				for (int m = 0; m < num; m++)
				{
					int negative = int.Parse(array20[m]);
					int positive = int.Parse(array21[m]);
					_SetAxis(m, array19[m], negative, positive);
					if (!string.IsNullOrEmpty(exAxisInverted))
					{
						if (array17[m] == "True")
						{
							_invertAxis[m] = true;
						}
						else
						{
							_invertAxis[m] = false;
						}
					}
				}
			}
			if (!string.IsNullOrEmpty(array16[1]))
			{
				string[] array22 = array16[1].Split('*');
				for (int n = 0; n < array22.Length - 1; n++)
				{
					_individualAxisSens[n] = float.Parse(array22[n]);
				}
			}
			if (array16.Length > 2 && !string.IsNullOrEmpty(array16[2]))
			{
				string[] array23 = array16[2].Split('*');
				for (int num2 = 0; num2 < array23.Length - 1; num2++)
				{
					_individualAxisGrav[num2] = float.Parse(array23[num2]);
				}
			}
			if (array16.Length > 3 && !string.IsNullOrEmpty(array16[3]))
			{
				string[] array24 = array16[3].Split('*');
				for (int num3 = 0; num3 < array24.Length - 1; num3++)
				{
					_individualAxisDead[num3] = float.Parse(array24[num3]);
				}
			}
		}
		else
		{
			if (!string.IsNullOrEmpty(_exAxis))
			{
				string exAxisInverted2 = _exAxisInverted;
				string[] array25 = ((!string.IsNullOrEmpty(exAxisInverted2)) ? exAxisInverted2.Split('*') : null);
				string[] array26 = _exAxis.Split('#');
				string[] array27 = array26[0].Split('*');
				string[] array28 = array26[1].Split('*');
				string[] array29 = array26[2].Split('*');
				int num4 = int.Parse(array26[3]);
				for (int num5 = 0; num5 < num4; num5++)
				{
					int negative2 = int.Parse(array28[num5]);
					int positive2 = int.Parse(array29[num5]);
					_SetAxis(num5, array27[num5], negative2, positive2);
					if (!string.IsNullOrEmpty(exAxisInverted2))
					{
						if (array25[num5] == "True")
						{
							_invertAxis[num5] = true;
						}
						else
						{
							_invertAxis[num5] = false;
						}
					}
				}
			}
			if (!string.IsNullOrEmpty(_exAxisSensitivity))
			{
				string[] array30 = _exAxisSensitivity.Split('*');
				for (int num6 = 0; num6 < array30.Length - 1; num6++)
				{
					_individualAxisSens[num6] = float.Parse(array30[num6]);
				}
			}
			if (!string.IsNullOrEmpty(_exAxisGravity))
			{
				string[] array31 = _exAxisGravity.Split('*');
				for (int num7 = 0; num7 < array31.Length - 1; num7++)
				{
					_individualAxisGrav[num7] = float.Parse(array31[num7]);
				}
			}
			if (!string.IsNullOrEmpty(_exAxisDeadzone))
			{
				string[] array32 = _exAxisDeadzone.Split('*');
				for (int num8 = 0; num8 < array32.Length - 1; num8++)
				{
					_individualAxisDead[num8] = float.Parse(array32[num8]);
				}
			}
		}
		if (!string.IsNullOrEmpty(_exCalibrations))
		{
			string[] array33 = _exCalibrations.Split('*');
			for (int num9 = 1; num9 <= array33.Length - 2; num9++)
			{
				_axisType[num9] = int.Parse(array33[num9]);
			}
		}
		if (!string.IsNullOrEmpty(_exCalibrationValues))
		{
			_CalibrationValuesFromString(_exCalibrationValues);
		}
		_cKeysLoaded = true;
	}

	public static void ResetInputs()
	{
		_cInputInit();
		for (int i = 0; i < _inputLength + 1; i++)
		{
			_SetKey(i, _defaultStrings[i, 0], _defaultStrings[i, 1], _defaultStrings[i, 2]);
			_modifierUsedPrimary[i] = _String2KeyCode(_defaultStrings[i, 3]);
			_modifierUsedSecondary[i] = _String2KeyCode(_defaultStrings[i, 4]);
		}
		for (int j = 0; j < _axisLength; j++)
		{
			_invertAxis[j] = false;
		}
		Clear();
		_SaveDefaults();
		_SaveInputs();
		_SaveAxInverted();
		if (cInput.OnKeyChanged != null)
		{
			cInput.OnKeyChanged();
		}
	}

	public static void Clear()
	{
		_cInputInit();
		Debug.LogWarning("Clearing out all cInput related values from PlayerPrefs");
		PlayerPrefs.DeleteKey("cInput_axInv");
		PlayerPrefs.DeleteKey("cInput_axis");
		PlayerPrefs.DeleteKey("cInput_indAxSens");
		PlayerPrefs.DeleteKey("cInput_indAxGrav");
		PlayerPrefs.DeleteKey("cInput_indAxDead");
		PlayerPrefs.DeleteKey("cInput_count");
		PlayerPrefs.DeleteKey("cInput_defaults");
		PlayerPrefs.DeleteKey("cInput_descr");
		PlayerPrefs.DeleteKey("cInput_inp");
		PlayerPrefs.DeleteKey("cInput_alt_inp");
		PlayerPrefs.DeleteKey("cInput_inpStr");
		PlayerPrefs.DeleteKey("cInput_alt_inpStr");
		PlayerPrefs.DeleteKey("cInput_dubl");
		PlayerPrefs.DeleteKey("cInput_saveCals");
		PlayerPrefs.DeleteKey("cInput_calsVals");
		PlayerPrefs.DeleteKey("cInput_modifierStr");
		PlayerPrefs.DeleteKey("cInput_alt_modifierStr");
	}

	private static bool _AxisInverted(int hash, bool invertedStatus, string description = "")
	{
		_cInputInit();
		int num = _FindAxisByHash(hash);
		if (num > -1)
		{
			_invertAxis[num] = invertedStatus;
			_SaveAxInverted();
			return invertedStatus;
		}
		string text = ((!string.IsNullOrEmpty(description)) ? description : ("axis with hashcode of " + hash));
		Debug.LogWarning("Couldn't find an axis match for " + text + " while trying to set inversion status. Is it possible you typed it wrong?");
		return false;
	}

	public static bool AxisInverted(string description, bool invertedStatus)
	{
		return _AxisInverted(description.GetHashCode(), invertedStatus, description);
	}

	public static bool AxisInverted(int descriptionHash, bool invertedStatus)
	{
		return _AxisInverted(descriptionHash, invertedStatus);
	}

	private static bool _AxisInverted(int hash, string description = "")
	{
		_cInputInit();
		int num = _FindAxisByHash(hash);
		if (num > -1)
		{
			return _invertAxis[num];
		}
		string text = ((!string.IsNullOrEmpty(description)) ? description : ("axis with hashcode of " + hash));
		Debug.LogWarning("Couldn't find an axis match for " + text + " while trying to get inversion status. Is it possible you typed it wrong?");
		return false;
	}

	public static bool AxisInverted(string description)
	{
		return _AxisInverted(description.GetHashCode(), description);
	}

	public static bool AxisInverted(int descriptionHash)
	{
		return _AxisInverted(descriptionHash);
	}

	public static void Calibrate()
	{
		_cInputInit();
		string text = "";
		_axisCalibrationOffset = _GetAxisRawValues();
		if (_usePlayerPrefs)
		{
			PlayerPrefs.SetString("cInput_calsVals", _CalibrationValuesToString());
		}
		for (int i = 1; i <= _numGamepads; i++)
		{
			for (int j = 1; j <= _numAxes; j++)
			{
				int num = _numAxes * (i - 1) + (j - 1);
				float axisRaw = Input.GetAxisRaw(_joyStrings[i, j]);
				_axisType[num] = ((axisRaw < 0f - deadzone) ? 1 : ((axisRaw > deadzone) ? (-1) : 0));
				text = text + _axisType[num] + "*";
				if (_usePlayerPrefs)
				{
					PlayerPrefs.SetString("cInput_saveCals", text);
				}
				_exCalibrations = text;
			}
		}
	}

	private static string _CalibrationValuesToString()
	{
		string text = "";
		foreach (KeyValuePair<string, float> item in _axisCalibrationOffset)
		{
			text = text + item.Key + "*" + item.Value + "#";
		}
		return text;
	}

	private static void _CalibrationValuesFromString(string calVals)
	{
		_axisCalibrationOffset.Clear();
		string[] array = calVals.Split('#');
		for (int i = 0; i < array.Length - 1; i++)
		{
			string[] array2 = array[i].Split('*');
			_axisCalibrationOffset.Add(array2[0], float.Parse(array2[1]));
		}
	}

	private static float _GetCalibratedAxisInput(string description)
	{
		float axisRaw = Input.GetAxisRaw(_ChangeStringToAxisName(description));
		switch (description)
		{
		case "Mouse Left":
		case "Mouse Right":
		case "Mouse Up":
		case "Mouse Down":
		case "Mouse Wheel Up":
		case "Mouse Wheel Down":
			return axisRaw;
		default:
		{
			if (_joyStringsPosIndices.ContainsKey(description))
			{
				_tmpCalibratedAxisIndices = _joyStringsPosIndices[description];
			}
			else
			{
				if (!_joyStringsNegIndices.ContainsKey(description))
				{
					Debug.LogWarning("No match found for " + description + " (" + _ChangeStringToAxisName(description) + "). This should never happen, in theory. Returning value of " + axisRaw);
					return axisRaw;
				}
				_tmpCalibratedAxisIndices = _joyStringsNegIndices[description];
			}
			int num = _numAxes * _tmpCalibratedAxisIndices[0] + _tmpCalibratedAxisIndices[1];
			switch (_axisType[num])
			{
			default:
				return axisRaw;
			case 1:
				return (axisRaw + 1f) / 2f;
			case -1:
				return (axisRaw - 1f) / 2f;
			}
		}
		}
	}

	private static KeyCode _String2KeyCode(string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return KeyCode.None;
		}
		if (_string2Key.Count == 0)
		{
			_CreateDictionary();
		}
		if (_string2Key.ContainsKey(str))
		{
			return _string2Key[str];
		}
		if (!_IsAxisValid(str))
		{
			Debug.Log("cInput error: " + str + " is not a valid input.");
		}
		return KeyCode.None;
	}

	private static bool _DefaultsExist()
	{
		if (_defaultStrings.Length <= 0)
		{
			return false;
		}
		return true;
	}

	private static bool _IsKeyDefined(int hash)
	{
		_cInputInit();
		return _inputNameHash.ContainsKey(hash);
	}

	public static bool IsKeyDefined(string keyName)
	{
		return _IsKeyDefined(keyName.GetHashCode());
	}

	public static bool IsKeyDefined(int keyHash)
	{
		return _IsKeyDefined(keyHash);
	}

	private static bool _IsAxisDefined(int hash)
	{
		_cInputInit();
		return _axisNameHash.ContainsKey(hash);
	}

	public static bool IsAxisDefined(string axisName)
	{
		return _IsAxisDefined(axisName.GetHashCode());
	}

	public static bool IsAxisDefined(int axisHash)
	{
		return _IsAxisDefined(axisHash);
	}

	private void _CheckDuplicates(int _num, int _count)
	{
		if (allowDuplicates)
		{
			return;
		}
		for (int i = 0; i < length; i++)
		{
			if (_count == 1)
			{
				if (_num != i && _inputPrimary[_num] == _inputPrimary[i] && _modifierUsedPrimary[_num] == _modifierUsedPrimary[i])
				{
					_inputPrimary[i] = KeyCode.None;
				}
				if (_inputPrimary[_num] == _inputSecondary[i] && _modifierUsedPrimary[_num] == _modifierUsedSecondary[i])
				{
					_inputSecondary[i] = KeyCode.None;
				}
			}
			if (_count == 2)
			{
				if (_inputSecondary[_num] == _inputPrimary[i] && _modifierUsedSecondary[_num] == _modifierUsedPrimary[i])
				{
					_inputPrimary[i] = KeyCode.None;
				}
				if (_num != i && _inputSecondary[_num] == _inputSecondary[i] && _modifierUsedSecondary[_num] == _modifierUsedSecondary[i])
				{
					_inputSecondary[i] = KeyCode.None;
				}
			}
		}
	}

	private void _CheckDuplicateStrings(int _num, int _count)
	{
		if (allowDuplicates)
		{
			return;
		}
		for (int i = 0; i < length; i++)
		{
			if (_count == 1)
			{
				if (_num != i && _axisPrimary[_num] == _axisPrimary[i])
				{
					_axisPrimary[i] = "";
					_inputPrimary[i] = KeyCode.None;
				}
				if (_axisPrimary[_num] == _axisSecondary[i])
				{
					_axisSecondary[i] = "";
					_inputSecondary[i] = KeyCode.None;
				}
			}
			if (_count == 2)
			{
				if (_axisSecondary[_num] == _axisPrimary[i])
				{
					_axisPrimary[i] = "";
					_inputPrimary[i] = KeyCode.None;
				}
				if (_num != i && _axisSecondary[_num] == _axisSecondary[i])
				{
					_axisSecondary[i] = "";
					_inputSecondary[i] = KeyCode.None;
				}
			}
		}
	}

	private void _CheckInputs()
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < _inputLength + 1; i++)
		{
			flag = Input.GetKey(_inputPrimary[i]);
			flag2 = Input.GetKey(_inputSecondary[i]);
			bool flag5 = false;
			bool flag6 = false;
			bool flag7 = false;
			for (int j = 0; j < _modifiers.Count; j++)
			{
				if (Input.GetKey(_modifiers[j]))
				{
					flag7 = true;
					if (!flag5 && _modifiers[j] == _modifierUsedPrimary[i])
					{
						flag5 = true;
					}
					if (!flag6 && _modifiers[j] == _modifierUsedSecondary[i])
					{
						flag6 = true;
					}
				}
			}
			bool flag8 = (_modifierUsedPrimary[i] == _inputPrimary[i] && !flag7) || (_modifierUsedPrimary[i] != _inputPrimary[i] && flag5);
			bool flag9 = (_modifierUsedSecondary[i] == _inputSecondary[i] && !flag7) || (_modifierUsedSecondary[i] != _inputSecondary[i] && flag6);
			if (!string.IsNullOrEmpty(_axisPrimary[i]))
			{
				flag3 = true;
				num = _GetCalibratedAxisInput(_axisPrimary[i]) * (float)_PosOrNeg(_axisPrimary[i]);
			}
			else
			{
				flag3 = false;
				num = (flag ? 1f : 0f);
			}
			if (!string.IsNullOrEmpty(_axisSecondary[i]))
			{
				flag4 = true;
				num2 = _GetCalibratedAxisInput(_axisSecondary[i]) * (float)_PosOrNeg(_axisSecondary[i]);
			}
			else
			{
				flag4 = false;
				num2 = (flag2 ? 1f : 0f);
			}
			if ((flag && flag8) || (flag2 && flag9) || (flag3 && num > deadzone) || (flag4 && num2 > deadzone))
			{
				_getKeyArray[i] = true;
			}
			else
			{
				_getKeyArray[i] = false;
			}
			if ((flag8 && Input.GetKeyDown(_inputPrimary[i])) || (flag9 && Input.GetKeyDown(_inputSecondary[i])))
			{
				_getKeyDownArray[i] = true;
			}
			else
			{
				bool flag10 = false;
				if (flag3 && num > deadzone && !_axisTriggerArrayPrimary[i])
				{
					_axisTriggerArrayPrimary[i] = true;
					flag10 = true;
				}
				if (flag4 && num2 > deadzone && !_axisTriggerArraySecondary[i])
				{
					_axisTriggerArraySecondary[i] = true;
					flag10 = true;
				}
				_getKeyDownArray[i] = (_axisTriggerArrayPrimary[i] || _axisTriggerArraySecondary[i]) && flag10;
			}
			if ((Input.GetKeyUp(_inputPrimary[i]) && flag8) || (Input.GetKeyUp(_inputSecondary[i]) && flag9))
			{
				_getKeyUpArray[i] = true;
			}
			else
			{
				bool flag11 = false;
				if (flag3 && num <= deadzone && _axisTriggerArrayPrimary[i])
				{
					_axisTriggerArrayPrimary[i] = false;
					flag11 = true;
				}
				if (flag4 && num2 <= deadzone && _axisTriggerArraySecondary[i])
				{
					_axisTriggerArraySecondary[i] = false;
					flag11 = true;
				}
				_getKeyUpArray[i] = (!_axisTriggerArrayPrimary[i] || !_axisTriggerArraySecondary[i]) && flag11;
			}
			float num3 = sensitivity;
			float num4 = gravity;
			float num5 = deadzone;
			sensitivity = ((_individualAxisSens[i] != -99f) ? _individualAxisSens[i] : num3);
			gravity = ((_individualAxisGrav[i] != -99f) ? _individualAxisGrav[i] : num4);
			deadzone = ((_individualAxisDead[i] != -99f) ? _individualAxisDead[i] : num5);
			float num6 = ((Time.deltaTime == 0f) ? 0.012f : Time.deltaTime);
			if (num > deadzone || num2 > deadzone)
			{
				_getAxisRaw[i] = Mathf.Max(num, num2);
				if (_getAxis[i] < _getAxisRaw[i])
				{
					_getAxis[i] = Mathf.Min(_getAxis[i] + sensitivity * num6, _getAxisRaw[i]);
				}
				if (_getAxis[i] > _getAxisRaw[i])
				{
					_getAxis[i] = Mathf.Max(_getAxisRaw[i], _getAxis[i] - gravity * num6);
				}
			}
			else
			{
				_getAxisRaw[i] = 0f;
				if (_getAxis[i] > 0f)
				{
					_getAxis[i] = Mathf.Max(0f, _getAxis[i] - gravity * num6);
				}
			}
			sensitivity = num3;
			gravity = num4;
			deadzone = num5;
		}
		for (int k = 0; k <= _axisLength; k++)
		{
			int num7 = _makeAxis[k, 0];
			int num8 = _makeAxis[k, 1];
			if (_makeAxis[k, 1] == -1)
			{
				_getAxisArray[k] = _getAxis[num7];
				_getAxisArrayRaw[k] = _getAxisRaw[num7];
			}
			else
			{
				_getAxisArray[k] = _getAxis[num8] - _getAxis[num7];
				_getAxisArrayRaw[k] = _getAxisRaw[num8] - _getAxisRaw[num7];
			}
		}
	}

	private void _InputScans()
	{
		KeyCode keyCode = KeyCode.None;
		if (Input.GetKey(KeyCode.Escape))
		{
			if (_cScanInput == 1)
			{
				_inputPrimary[_cScanIndex] = KeyCode.None;
				_axisPrimary[_cScanIndex] = "";
				_cScanInput = 0;
			}
			if (_cScanInput == 2)
			{
				_inputSecondary[_cScanIndex] = KeyCode.None;
				_axisSecondary[_cScanIndex] = "";
				_cScanInput = 0;
			}
		}
		if (_scanning && Input.anyKeyDown && !Input.GetKey(KeyCode.Escape))
		{
			KeyCode keyCode2 = KeyCode.None;
			for (int i = 0; i < 450; i++)
			{
				KeyCode keyCode3 = (KeyCode)i;
				if (keyCode3.ToString().StartsWith("Mouse"))
				{
					if (!_allowMouseButtons)
					{
						continue;
					}
				}
				else if (keyCode3.ToString().StartsWith("Joystick"))
				{
					if (!_allowJoystickButtons)
					{
						continue;
					}
				}
				else if (!_allowKeyboard)
				{
					continue;
				}
				for (int j = 0; j < _modifiers.Count; j++)
				{
					if (Input.GetKeyDown(_modifiers[j]))
					{
						return;
					}
					if (!Input.GetKeyDown(keyCode3))
					{
						continue;
					}
					keyCode2 = keyCode3;
					keyCode = keyCode3;
					bool flag = false;
					for (int k = 0; k < _markedAsAxis.Count; k++)
					{
						if (_cScanIndex == _markedAsAxis[k])
						{
							flag = true;
							break;
						}
					}
					if (Input.GetKey(_modifiers[j]) && !flag)
					{
						keyCode = _modifiers[j];
						break;
					}
				}
			}
			if (keyCode2 != 0)
			{
				bool flag2 = true;
				for (int l = 0; l < _forbiddenKeys.Count; l++)
				{
					if (keyCode2 == _forbiddenKeys[l])
					{
						flag2 = false;
						break;
					}
				}
				if (flag2)
				{
					if (_cScanInput == 1)
					{
						_inputPrimary[_cScanIndex] = keyCode2;
						_modifierUsedPrimary[_cScanIndex] = keyCode;
						_axisPrimary[_cScanIndex] = "";
						_CheckDuplicates(_cScanIndex, _cScanInput);
					}
					if (_cScanInput == 2)
					{
						_inputSecondary[_cScanIndex] = keyCode2;
						_modifierUsedSecondary[_cScanIndex] = keyCode;
						_axisSecondary[_cScanIndex] = "";
						_CheckDuplicates(_cScanIndex, _cScanInput);
					}
					_cScanInput = 0;
				}
			}
		}
		if (_allowMouseButtons)
		{
			if (Input.GetAxis("Mouse Wheel") > 0f && !Input.GetKey(KeyCode.Escape))
			{
				if (!_forbiddenAxes.Contains("Mouse Wheel Up"))
				{
					if (_cScanInput == 1)
					{
						_axisPrimary[_cScanIndex] = "Mouse Wheel Up";
					}
					if (_cScanInput == 2)
					{
						_axisSecondary[_cScanIndex] = "Mouse Wheel Up";
					}
					_CheckDuplicateStrings(_cScanIndex, _cScanInput);
					_cScanInput = 0;
				}
			}
			else if (Input.GetAxis("Mouse Wheel") < 0f && !Input.GetKey(KeyCode.Escape) && !_forbiddenAxes.Contains("Mouse Wheel Down"))
			{
				if (_cScanInput == 1)
				{
					_axisPrimary[_cScanIndex] = "Mouse Wheel Down";
				}
				if (_cScanInput == 2)
				{
					_axisSecondary[_cScanIndex] = "Mouse Wheel Down";
				}
				_CheckDuplicateStrings(_cScanIndex, _cScanInput);
				_cScanInput = 0;
			}
		}
		if (_allowMouseAxis)
		{
			if (Input.GetAxis("Mouse Horizontal") < 0f - deadzone && !Input.GetKey(KeyCode.Escape))
			{
				if (!_forbiddenAxes.Contains("Mouse Left"))
				{
					if (_cScanInput == 1)
					{
						_axisPrimary[_cScanIndex] = "Mouse Left";
					}
					if (_cScanInput == 2)
					{
						_axisSecondary[_cScanIndex] = "Mouse Left";
					}
					_CheckDuplicateStrings(_cScanIndex, _cScanInput);
					_cScanInput = 0;
				}
			}
			else if (Input.GetAxis("Mouse Horizontal") > deadzone && !Input.GetKey(KeyCode.Escape) && !_forbiddenAxes.Contains("Mouse Right"))
			{
				if (_cScanInput == 1)
				{
					_axisPrimary[_cScanIndex] = "Mouse Right";
				}
				if (_cScanInput == 2)
				{
					_axisSecondary[_cScanIndex] = "Mouse Right";
				}
				_CheckDuplicateStrings(_cScanIndex, _cScanInput);
				_cScanInput = 0;
			}
			if (Input.GetAxis("Mouse Vertical") > deadzone && !Input.GetKey(KeyCode.Escape))
			{
				if (!_forbiddenAxes.Contains("Mouse Up"))
				{
					if (_cScanInput == 1)
					{
						_axisPrimary[_cScanIndex] = "Mouse Up";
					}
					if (_cScanInput == 2)
					{
						_axisSecondary[_cScanIndex] = "Mouse Up";
					}
					_CheckDuplicateStrings(_cScanIndex, _cScanInput);
					_cScanInput = 0;
				}
			}
			else if (Input.GetAxis("Mouse Vertical") < 0f - deadzone && !Input.GetKey(KeyCode.Escape) && !_forbiddenAxes.Contains("Mouse Down"))
			{
				if (_cScanInput == 1)
				{
					_axisPrimary[_cScanIndex] = "Mouse Down";
				}
				if (_cScanInput == 2)
				{
					_axisSecondary[_cScanIndex] = "Mouse Down";
				}
				_CheckDuplicateStrings(_cScanIndex, _cScanInput);
				_cScanInput = 0;
			}
		}
		if (!_allowJoystickAxis)
		{
			return;
		}
		float num = 0.25f;
		for (int m = 1; m <= _numGamepads; m++)
		{
			for (int n = 1; n <= _numAxes; n++)
			{
				string text = _joyStrings[m, n];
				string text2 = _joyStringsPos[m, n];
				string text3 = _joyStringsNeg[m, n];
				float axisRaw = Input.GetAxisRaw(text);
				bool flag3 = false;
				if (_axisRawValues.ContainsKey(text) && !Mathf.Approximately(_axisRawValues[text], axisRaw))
				{
					flag3 = true;
				}
				if (!flag3)
				{
					continue;
				}
				float num2;
				if (axisRaw < 0f)
				{
					if (_forbiddenAxes.Contains(text3))
					{
						continue;
					}
					num2 = _GetCalibratedAxisInput(text3);
				}
				else
				{
					if (_forbiddenAxes.Contains(text2))
					{
						continue;
					}
					num2 = _GetCalibratedAxisInput(text2);
				}
				if (!_scanning || !(Mathf.Abs(num2) > num) || Input.GetKey(KeyCode.Escape))
				{
					continue;
				}
				if (_cScanInput == 1)
				{
					if (num2 > num)
					{
						_axisPrimary[_cScanIndex] = text2;
					}
					else if (num2 < 0f - num)
					{
						_axisPrimary[_cScanIndex] = text3;
					}
					_CheckDuplicateStrings(_cScanIndex, _cScanInput);
					_cScanInput = 0;
					return;
				}
				if (_cScanInput == 2)
				{
					if (num2 > num)
					{
						_axisSecondary[_cScanIndex] = text2;
					}
					else if (num2 < 0f - num)
					{
						_axisSecondary[_cScanIndex] = text3;
					}
					_CheckDuplicateStrings(_cScanIndex, _cScanInput);
					_cScanInput = 0;
					return;
				}
			}
		}
	}
}
