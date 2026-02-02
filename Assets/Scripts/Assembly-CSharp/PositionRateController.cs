using UnityEngine;

public class PositionRateController : MonoBehaviour
{
    [SerializeField]
    private Transform trans;

    [SerializeField]
    private float rate;

    [SerializeField]
    private Transform lWeaponPoint;

    public float Rate
    {
        set
        {
            rate = value;
        }
    }

    private void Awake()
    {
        if (trans == null)
        {
            base.enabled = false;
        }
    }

    private void LateUpdate()
    {
        trans.localPosition *= rate;
        if (lWeaponPoint != null)
        {
            lWeaponPoint.localPosition = new Vector3(lWeaponPoint.localPosition.x * rate, lWeaponPoint.localPosition.y * rate, lWeaponPoint.localPosition.z);
        }
    }

    public float GetRate()
    {
        return rate;
    }
}
