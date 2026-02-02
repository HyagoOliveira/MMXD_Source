#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CodeStage.AntiCheat.ObscuredTypes;
using Kino;
using StageLib;
using UnityEngine;
using enums;

public class OrangeBattleUtility : MonoBehaviourSingleton<OrangeBattleUtility>
{
    public static readonly Quaternion QuaternionNormal = Quaternion.Euler(0f, 0f, 0f);

    public static readonly Quaternion QuaternionReverse = Quaternion.Euler(0f, 180f, 0f);

    public static readonly float PPU = 1f / 24f;

    public static readonly float FPS = 60f;

    public static readonly float Gravity = -0.25f * PPU * FPS * FPS;

    public static readonly float MaxGravity = -5.75f * PPU * FPS;

    public static float PlayerWalkSpeed;

    public static float PlayerJumpSpeed;

    public static float PlayerDashSpeed;

    public static readonly VInt FP_PPU = new VInt(1f / 24f);

    public static readonly VInt FP_FPS = new VInt(60f);

    public static readonly VInt FP_Gravity = new VInt(-0.25f * PPU * FPS * FPS);

    public static readonly VInt FP_MaxGravity = new VInt(-5.75f * PPU * FPS);

    public static List<OrangeCharacter> ListPlayer = new List<OrangeCharacter>();

    public static OrangeCharacter CurrentCharacter;

    private static SRandom random = new SRandom(1u);

    public static bool GlobalInvincible = false;

    public static float HitStopTime = 2f / 15f;

    public static VInt3 GlobalVelocityExtra = VInt3.zero;

    public static VInt3 GlobalGravityExtra = VInt3.zero;

    private Dictionary<int, BloomImageStruct> DicBloomStrcut = new Dictionary<int, BloomImageStruct>();

    private BloomImageStruct defaultBloomStrcut;

    private static readonly RaycastHit2D EmptyHit2D = default(RaycastHit2D);

    private static Vector3 _targetScreenPos = Vector2.zero;

    private static Vector2 _relativeResolution;

    private static Vector2 _screenResolution;

    private static OrangeTimer _lockWallKickTimer;

    private static int _lockWallJumpDuration;

    private static Coroutine _lockWallJumpCoroutine;

    private static readonly EventManager.StageEventCall StageEventCall = new EventManager.StageEventCall();

    private void OnEnable()
    {
        Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.STAGE_UPDATE_PLAYER_LIST, UpdatePlayer);
    }

    private void OnDisable()
    {
        Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.STAGE_UPDATE_PLAYER_LIST, UpdatePlayer);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Clear();
    }

    public void OnChangeScene()
    {
        GlobalInvincible = false;
        GlobalVelocityExtra = VInt3.zero;
        GlobalGravityExtra = VInt3.zero;
        InitLockWallJump();
        MonoBehaviourSingleton<EnemyHumanResourceManager>.Instance.Initialize();
    }

    public void Awake()
    {
        UpdatePlayerParameters();
        BloomOnImageDictInit();
        Singleton<GenericEventManager>.Instance.AttachEvent<OrangeCharacter>(EventManager.ID.LOCAL_PLAYER_SPWAN, EventPlayerSpawn);
        _screenResolution = new Vector2(MonoBehaviourSingleton<OrangeGameManager>.Instance.ScreenWidth, MonoBehaviourSingleton<OrangeGameManager>.Instance.ScreenHeight);
        _relativeResolution = new Vector2(_screenResolution.x, _screenResolution.x * 0.75f);
    }

    public static Transform FindChildRecursive(ref Transform[] target, string key, bool equal = false)
    {
        if (!equal)
        {
            return target.FirstOrDefault((Transform t) => t.name.Contains(key));
        }
        return target.FirstOrDefault((Transform t) => t.name.Equals(key));
    }

    public static Transform FindChildRecursive(Transform[] target, string key, bool equal = false)
    {
        if (!equal)
        {
            return target.FirstOrDefault((Transform t) => t.name.Contains(key));
        }
        return target.FirstOrDefault((Transform t) => t.name.Equals(key));
    }

    public static Transform FindChildRecursive(Transform target, string key, bool equal = false)
    {
        if (!equal)
        {
            return target.GetComponentsInChildren<Transform>(true).FirstOrDefault((Transform t) => t.name.Contains(key));
        }
        return target.GetComponentsInChildren<Transform>(true).FirstOrDefault((Transform t) => t.name.Equals(key));
    }

    public static Transform[] FindMultiChildRecursive(Transform[] target, string key, bool equal = false)
    {
        List<Transform> list = new List<Transform>();
        foreach (Transform target2 in target)
        {
            list.AddRange(FindAllChildRecursive(target2, key, equal));
        }
        return list.ToArray();
    }

    public static Transform[] FindAllChildRecursive(ref Transform[] target, string key, bool equal = false)
    {
        if (!equal)
        {
            return target.Where((Transform x) => x.name.Contains(key)).ToArray();
        }
        return target.Where((Transform x) => x.name.Equals(key)).ToArray();
    }

    public static Transform[] FindAllChildRecursive(Transform[] target, string key, bool equal = false)
    {
        if (!equal)
        {
            return target.Where((Transform x) => x.name.Contains(key)).ToArray();
        }
        return target.Where((Transform x) => x.name.Equals(key)).ToArray();
    }

    public static Transform[] FindAllChildRecursive(Transform target, string key, bool equal = false)
    {
        if (!equal)
        {
            return (from t in target.GetComponentsInChildren<Transform>(true)
                    where t.name.Contains(key)
                    select t).ToArray();
        }
        return (from t in target.GetComponentsInChildren<Transform>(true)
                where t.name.Equals(key)
                select t).ToArray();
    }

    public static void UpdatePlayerParameters()
    {
        PlayerWalkSpeed = PlayerPrefs.GetFloat("DEBUG_WALKSPEED", 1.75f);
        PlayerJumpSpeed = PlayerPrefs.GetFloat("DEBUG_JUMPSPEED", 5.3242188f);
        PlayerDashSpeed = PlayerPrefs.GetFloat("DEBUG_DASHSPEED", 3.4570312f);
        OrangeCharacter.UpdatePlayerParameters();
    }

    public static void AddBloom(GameObject go)
    {
        /*if ((bool)go.GetComponent<Bloom>())
		{
			UnityEngine.Object.Destroy(go.GetComponent<Bloom>());
		}
		Bloom bloom = go.AddComponent<Bloom>();
		bloom.thresholdGamma = 1.5f;
		bloom.softKnee = 0.1f;
		bloom.intensity = 0.2f;
		bloom.radius = 2f;*/
    }

    private void BloomOnImageDictInit()
    {
        defaultBloomStrcut = default(BloomImageStruct);
        defaultBloomStrcut.color = new Color(0.25f, 0.25f, 0.25f, 1f);
        defaultBloomStrcut.radius = 3f;
        DicBloomStrcut = new Dictionary<int, BloomImageStruct>();
        AddBloomImageStructToDic(102, new Color(0.25f, 0.25f, 0.25f, 1f), 1f);
    }

    private void AddBloomImageStructToDic(int key, Color color, float radius)
    {
        BloomImageStruct p_value = default(BloomImageStruct);
        p_value.color = color;
        p_value.radius = radius;
        DicBloomStrcut.ContainsAdd(key, p_value);
    }

    public static BloomOnImage AddBloomOnImage(GameObject go, GameObject objectWithImage)
    {
        BloomOnImage bloomOnImage = go.AddComponent<BloomOnImage>();
        bloomOnImage.thresholdGamma = 1.5f;
        bloomOnImage.softKnee = 0.1f;
        bloomOnImage.intensity = 0.5f;
        bloomOnImage.radius = MonoBehaviourSingleton<OrangeBattleUtility>.Instance.defaultBloomStrcut.radius;
        bloomOnImage.highQuality = false;
        bloomOnImage.antiFlicker = false;
        bloomOnImage._objectWithImage = objectWithImage;
        bloomOnImage.Init(MonoBehaviourSingleton<OrangeBattleUtility>.Instance.defaultBloomStrcut.color);
        return bloomOnImage;
    }

    public static BloomOnImage AddBloomOnImage(GameObject go, GameObject objectWithImage, int CharacterId)
    {
        BloomOnImage bloomOnImage = go.AddComponent<BloomOnImage>();
        BloomImageStruct value;
        if (!MonoBehaviourSingleton<OrangeBattleUtility>.Instance.DicBloomStrcut.TryGetValue(CharacterId, out value))
        {
            value = MonoBehaviourSingleton<OrangeBattleUtility>.Instance.defaultBloomStrcut;
        }
        bloomOnImage.thresholdGamma = 1.5f;
        bloomOnImage.softKnee = 0.1f;
        bloomOnImage.intensity = 0.5f;
        bloomOnImage.radius = value.radius;
        bloomOnImage.highQuality = false;
        bloomOnImage.antiFlicker = false;
        bloomOnImage._objectWithImage = objectWithImage;
        bloomOnImage.Init(value.color);
        return bloomOnImage;
    }

    public float GetBloomRadius(int CharacterId)
    {
        BloomImageStruct value;
        if (!MonoBehaviourSingleton<OrangeBattleUtility>.Instance.DicBloomStrcut.TryGetValue(CharacterId, out value))
        {
            value = MonoBehaviourSingleton<OrangeBattleUtility>.Instance.defaultBloomStrcut;
        }
        return value.radius;
    }

    public static bool LockPlayer()
    {
        return true;
    }

    public static bool UnlockPlayer()
    {
        if (CurrentCharacter == null)
        {
            return false;
        }
        CurrentCharacter.LockInput = false;
        return true;
    }

    public void EventPlayerSpawn(OrangeCharacter tPlayer)
    {
        CurrentCharacter = tPlayer;
    }

    public static Vector2 RadianToVector2(float radian)
    {
        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
    }

    public static Vector2 DegreeToVector2(float degree)
    {
        return RadianToVector2(degree * ((float)Math.PI / 180f));
    }

    public static void SetWeaponParent(Transform transform, int weaponType, ref Transform[] target)
    {
        Vector3 zero = Vector3.zero;
        switch ((WeaponType)(short)weaponType)
        {
            case WeaponType.Buster:
            case WeaponType.Spray:
            case WeaponType.SprayHeavy:
                transform.SetParent(FindChildRecursive(ref target, "L BusterPoint"));
                transform.localPosition = zero;
                transform.localEulerAngles = zero;
                break;
            case WeaponType.Melee:
                transform.SetParent(FindChildRecursive(ref target, "R WeaponPoint"));
                transform.localPosition = zero;
                transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                break;
            case WeaponType.DualGun:
            case WeaponType.MGun:
                transform.SetParent(FindChildRecursive(ref target, "R WeaponPoint"));
                transform.localPosition = zero;
                transform.localEulerAngles = zero;
                break;
            case WeaponType.Gatling:
            case WeaponType.Launcher:
                transform.SetParent(FindChildRecursive(ref target, "L WeaponPoint"));
                transform.localPosition = zero;
                transform.localEulerAngles = zero;
                break;
        }
        if (transform.parent != null)
        {
            OrangeBattleUtility.ChangeLayersRecursively<Transform>(transform, transform.parent.gameObject.layer);
        }
        else
        {
            Debug.LogError("Weapon Parent is null");
        }
    }

    public static void SetWeaponParentSp(Transform transformWeapon, int weaponType, Transform transformParent)
    {
        Vector3 localPosition = new Vector3(0f, 1f, -0.8f);
        Quaternion localRotation = Quaternion.Euler(0f, -90f, 0f);
        switch ((WeaponType)(short)weaponType)
        {
            case WeaponType.Buster:
                localRotation = Quaternion.Euler(33.4f, -58.5f, 0f);
                break;
            case WeaponType.Spray:
            case WeaponType.SprayHeavy:
                localPosition = new Vector3(0.1f, 0.75f, -0.8f);
                localRotation = Quaternion.Euler(-58.9f, -90f, 0f);
                break;
            case WeaponType.Melee:
                localPosition = new Vector3(0f, 1.5f, -0.8f);
                localRotation = Quaternion.Euler(70f, -90f, 0f);
                break;
            case WeaponType.MGun:
                localPosition = new Vector3(0.15f, 1.2f, -0.8f);
                localRotation = Quaternion.Euler(-302.8f, -90f, 0f);
                break;
            case WeaponType.Launcher:
                localPosition = new Vector3(-0.32f, 0.845f, -0.59f);
                localRotation = Quaternion.Euler(-22.38f, -65.36f, -9.91f);
                break;
        }
        transformWeapon.SetParent(transformParent);
        transformWeapon.localPosition = localPosition;
        transformWeapon.localRotation = localRotation;
        OrangeBattleUtility.ChangeLayersRecursively<Transform>(transformWeapon, transformWeapon.parent.gameObject.layer);
    }

    public static bool GetHandMeshEnable(int weaponType)
    {
        bool result = true;
        switch ((WeaponType)(short)weaponType)
        {
            case WeaponType.Buster:
            case WeaponType.Spray:
            case WeaponType.SprayHeavy:
                result = false;
                break;
        }
        return result;
    }

    public static void ChangeLayersRecursively(Transform trans, int layer, bool chageSelf = true)
    {
        if (chageSelf)
        {
            trans.gameObject.layer = layer;
        }
        foreach (Transform tran in trans)
        {
            OrangeBattleUtility.ChangeLayersRecursively<Transform>(tran, layer);
        }
    }

    public static void ChangeLayersRecursively<T>(T trans, int layer)
    {
        Transform obj = trans as Transform;
        obj.gameObject.layer = layer;
        foreach (T item in obj)
        {
            ChangeLayersRecursively(item, layer);
        }
    }

    public static int Random(int min, int max)
    {
        return random.Range(min, max);
    }

    public static float Random(float min, float max)
    {
        return UnityEngine.Random.Range(min, max);
    }

    private static void UpdatePlayer()
    {
        ListPlayer = StageUpdate.runPlayers;
    }

    public static OrangeCharacter GetRandomPlayer()
    {
        if (ListPlayer.Count <= 0)
        {
            return null;
        }
        return ListPlayer[Random(0, ListPlayer.Count)];
    }

    public static void UpdateSkillCD(WeaponStruct weaponStruct)
    {
        SKILL_TABLE bulletData = weaponStruct.BulletData;
        int num = 0;
        if (bulletData.n_MAGAZINE_TYPE == 1)
        {
            num = -1;
        }
        weaponStruct.MagazineRemain -= bulletData.n_USE_COST;
        if (weaponStruct.MagazineRemain < (float)num)
        {
            weaponStruct.MagazineRemain = num;
        }
        weaponStruct.LastUseTimer.TimerStart();
    }

    public static void UpdateSkillCD(WeaponStruct weaponStruct, float cost, float minMagazineRemain = 0f)
    {
        weaponStruct.MagazineRemain -= cost;
        if (weaponStruct.MagazineRemain < minMagazineRemain)
        {
            weaponStruct.MagazineRemain = minMagazineRemain;
        }
        weaponStruct.LastUseTimer.TimerStart();
    }

    public static void UpdateSkillMeasure(OrangeCharacter _refEntity, WeaponStruct weaponStruct)
    {
        _refEntity.selfBuffManager.AddMeasure(-weaponStruct.BulletData.n_USE_COST);
    }

    public static void UpdateEnemyHp(ref ObscuredInt hpNow, ref ObscuredInt dmg, Action UpdateHurtAction)
    {
        hpNow = (int)hpNow - (int)dmg;
        UpdateHurtAction();
    }

    public static void UpdateEnemyHp(ref ObscuredInt hpNow, ref ObscuredInt dmg)
    {
        hpNow = (int)hpNow - (int)dmg;
    }

    public static RaycastHit2D RaycastIgnoreSelf(Vector2 origin, Vector2 direction, float distance, int layerMask, Transform selfTransform)
    {
        RaycastHit2D[] array = Physics2D.RaycastAll(origin, direction, distance, layerMask);
        for (int i = 0; i < array.Length; i++)
        {
            RaycastHit2D result = array[i];
            if (!result.transform.IsChildOf(selfTransform.root))
            {
                return result;
            }
        }
        return EmptyHit2D;
    }

    public static void Clear()
    {
        ListPlayer.Clear();
        CurrentCharacter = null;
    }

    public static bool IsInsideScreen(Vector3 targetWorldPosition)
    {
        _targetScreenPos = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.WorldToScreenPoint(targetWorldPosition);
        if (!(_targetScreenPos.x < 0f) && !(_targetScreenPos.x > _relativeResolution.x) && !(_targetScreenPos.y < 0f))
        {
            return !(_targetScreenPos.y > _relativeResolution.y);
        }
        return false;
    }

    public static void AddBulletExtraCollision(BasicBullet bullet, out BulletExtraCollision bulletExtraCollision)
    {
        Transform transform = FindChildRecursive(bullet.transform, "BulletExtraCollision", true);
        if (transform == null)
        {
            GameObject obj = new GameObject("BulletExtraCollision");
            obj.layer = bullet.transform.gameObject.layer;
            obj.transform.SetParent(bullet.transform);
            obj.transform.localPosition = Vector3.zero;
            transform = obj.transform;
        }
        bulletExtraCollision = transform.gameObject.AddOrGetComponent<BulletExtraCollision>();
        bulletExtraCollision.MasterBullet = bullet;
    }

    public static void AddEnemyAutoAimSystem(Transform parent, out EnemyAutoAimSystem enemyAutoAimSystem)
    {
        Transform transform = FindChildRecursive(parent, "AutoAimSystem", true);
        if (transform == null)
        {
            GameObject obj = new GameObject("AutoAimSystem");
            obj.transform.SetParent(parent);
            obj.transform.localPosition = Vector3.zero;
            transform = obj.transform;
        }
        enemyAutoAimSystem = transform.gameObject.AddOrGetComponent<EnemyAutoAimSystem>();
    }

    public static void AddNeutralAutoAimSystem(Transform parent, out NeutralAutoAimSystem autoAimSystem)
    {
        Transform transform = FindChildRecursive(parent, "AutoAimSystem", true);
        if (transform == null)
        {
            GameObject obj = new GameObject("AutoAimSystem");
            obj.transform.SetParent(parent);
            obj.transform.localPosition = Vector3.zero;
            transform = obj.transform;
        }
        autoAimSystem = transform.gameObject.AddOrGetComponent<NeutralAutoAimSystem>();
    }

    private void InitLockWallJump(params object[] pParam)
    {
        StageEventCall.nID = 991;
        if (_lockWallKickTimer == null)
        {
            _lockWallKickTimer = OrangeTimerManager.GetStaticTimer();
        }
        _lockWallKickTimer.TimerStop();
        _lockWallJumpDuration = 0;
    }

    public void SetLockWallJump(int duration = 3000)
    {
        _lockWallJumpDuration = duration;
        if (_lockWallJumpCoroutine != null)
        {
            _lockWallKickTimer.TimerStart();
            return;
        }
        _lockWallJumpCoroutine = StartCoroutine(LockWallJumpCoroutine());
        Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.LOCK_WALL_JUMP);
    }

    private static IEnumerator LockWallJumpCoroutine()
    {
        _lockWallKickTimer.TimerStart();
        Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, StageEventCall);
        while (_lockWallKickTimer.GetMillisecond() < _lockWallJumpDuration)
        {
            yield return CoroutineDefine._waitForEndOfFrame;
        }
        Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, StageEventCall);
        _lockWallJumpCoroutine = null;
    }

    public void CallSummonEnemyEvent(Transform caller, int eventID = 999)
    {
        EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
        stageEventCall.nID = eventID;
        stageEventCall.nStageEvent = STAGE_EVENT.STAGE_START_SUMMON_ENEMY;
        stageEventCall.tTransform = caller;
        Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
    }

    public void ChangeRenderLayer(GameObject[] RenderModes, int layer)
    {
        if (RenderModes != null)
        {
            for (int i = 0; i < RenderModes.Length; i++)
            {
                RenderModes[i].layer = layer;
            }
        }
    }

    public void WeaponForceRotate(GameObject p_go)
    {
        WeaponForceRotate component = p_go.GetComponent<WeaponForceRotate>();
        if (component != null)
        {
            component.TriggerRotate();
        }
    }

    public Vector3 GetShootDirection(OrangeCharacter _refEntity, Transform shootTransform, int x = 1, int y = 1, int z = 1)
    {
        Vector3 vector = _refEntity.ShootDirection;
        if ((bool)_refEntity.PlayerAutoAimSystem && _refEntity.UseAutoAim && _refEntity.PlayerAutoAimSystem.AutoAimTarget != null)
        {
            vector = (_refEntity.PlayerAutoAimSystem.GetTargetPoint().xy() - shootTransform.position.xy()).normalized;
        }
        return new Vector3((float)x * vector.x, (float)y * vector.y, (float)z * vector.z).normalized;
    }

    public static OrangeCharacter GetHitTargetOrangeCharacter(Collider2D hitTarget)
    {
        if (hitTarget == null)
        {
            return null;
        }
        OrangeCharacter component = hitTarget.GetComponent<OrangeCharacter>();
        if (component != null)
        {
            return component;
        }
        StageObjParam component2 = hitTarget.GetComponent<StageObjParam>();
        if (component2 != null && component2.tLinkSOB != null)
        {
            return StageUpdate.GetPlayerByID(component2.tLinkSOB.sNetSerialID);
        }
        PlayerCollider component3 = hitTarget.GetComponent<PlayerCollider>();
        if (component3 != null && component3.IsDmgReduceShield())
        {
            Transform dmgReduceOwnerTransform = component3.GetDmgReduceOwnerTransform();
            if (dmgReduceOwnerTransform != null)
            {
                return dmgReduceOwnerTransform.GetComponent<OrangeCharacter>();
            }
        }
        return null;
    }
}
