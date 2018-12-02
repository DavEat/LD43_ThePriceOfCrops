using UnityEngine;

public class Crops : MonoBehaviour
{
    #region Vars
    protected CropsData _cropsData;
    protected float growEnd;

    [HideInInspector] public bool eatable = true;

    public Transform _transform { get; private set; }
    #endregion
    #region MonoFunctions
    private void Start()
    {
        _transform = GetComponent<Transform>();
    }
    private void Update()
    {
        if (eatable)
            Grow();
    }
    #endregion
    #region Functions
    public void Init(CropsData data)
    {
        _cropsData = data;

        growEnd = Time.time + _cropsData.growTime;
    }
    public bool CanBeHarvest()
    {
        return growEnd < Time.time;
    }
    public Crops Harvest()
    {
        return this;
    }
    protected virtual void Grow()
    {
        _transform.localScale = Vector3.one * Mathf.Lerp(.1f, 1, 1 - Mathf.Min((growEnd - Time.time) / _cropsData.growTime, 1));
    }
    public int GetFoodValue()
    {
        return _cropsData.food;
    }
    public int GetCropId()
    {
        return _cropsData.id;
    }
    public void CropsDamaged()
    {
        eatable = false;
        if (_cropsData.damaged != null)
        {
            GetComponent<MeshFilter>().mesh = _cropsData.damaged;
            if (_cropsData.name.ToLower().Equals("pumpkin"))
            {
                _transform.localEulerAngles = Vector3.up * Random.Range(0, 360);
                _transform.GetChild(0).gameObject.SetActive(false);
            }
        }
    }
    public void Drag(Transform parent)
    {
        _transform.parent = parent;
        _transform.localPosition = Vector3.zero;
        _transform.localRotation = Quaternion.identity;

        if (_cropsData.name.ToLower().Equals("pumpkin"))
        {
            _transform.GetChild(0).gameObject.SetActive(false);
        }
    }
    #endregion
}
