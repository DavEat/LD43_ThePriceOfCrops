using UnityEngine;

public class Crops : MonoBehaviour
{
    #region Vars
    protected CropsData _cropsData;
    protected float growEnd;

    public Transform _transform { get; private set; }
    #endregion
    #region MonoFunctions
    private void Start()
    {
        _transform = GetComponent<Transform>();
    }
    private void Update()
    {
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
    #endregion
}
