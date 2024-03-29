﻿using UnityEngine;

public class Crops : MonoBehaviour
{
    #region Vars
    protected CropsData _cropsData;
    protected float growEnd;

    [HideInInspector] public bool eatable = true;

    public Transform _transform { get; private set; }

    public bool needToGrow = true;
    #endregion
    #region MonoFunctions
    private void Start()
    {
        _transform = GetComponent<Transform>();
    }
    private void Update()
    {
        if (eatable && needToGrow)
            Grow();
    }
    #endregion
    #region Functions
    public void Init(CropsData data)
    {
        _cropsData = data;

        growEnd = GameManager.time + _cropsData.growTime;
    }
    public bool CanBeHarvest()
    {
        return growEnd < GameManager.time;
    }
    public Crops Harvest()
    {
        return this;
    }
    protected virtual void Grow()
    {
        _transform.localScale = Vector3.one * Mathf.Lerp(.1f, 1, 1 - Mathf.Min((growEnd - GameManager.time) / _cropsData.growTime, 1));
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
            if (_cropsData.Name.ToLower().Equals("pumpkin"))
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

        if (_cropsData.Name.ToLower().Equals("pumpkin"))
        {
            _transform.GetChild(0).gameObject.SetActive(false);
        }
        else if (_cropsData.Name.ToLower().Equals("wheat"))
        {
            GetComponent<MeshFilter>().mesh = _cropsData.harvested;
        }
    }
    #endregion
}
