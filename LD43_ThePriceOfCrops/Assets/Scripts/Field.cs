using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour {

    #region Vars
    [SerializeField] private Transform _cropsParent;
    [SerializeField] private Vector2Int _fieldSize = new Vector2Int(5, 5);

    [HideInInspector] public CropsData cropsData;

    private PlantPoint[] _plantPoints;

    [SerializeField] private GameObject _interface;
    #endregion
    #region MonoFunctions
    private void Start ()
    {
        InitPlantPoint();
        cropsData = FoodManager.inst.GetCrops(1);
    }
	private void Update ()
    {
		
	}
    #endregion
    #region Functions
    public void ToggleInterface(bool value)
    {
        if (_interface.activeSelf)
            _interface.SetActive(false);
        else _interface.SetActive(value);
    }
    public void SelectCrops(int id)
    {
        cropsData = FoodManager.inst.GetCrops(id);
    }
    private void InitPlantPoint()
    {
        _plantPoints = new PlantPoint[_fieldSize.x * _fieldSize.y];
        for (int y = 0; y < _fieldSize.y; y++)
        {
            for (int x = 0; x < _fieldSize.x; x++)
            {
                int index = y * _fieldSize.x + x;
                int xPos = (y % 2 == 0 ? (x * 2) : ((_fieldSize.x - 1) - x) * 2);
                _plantPoints[index] = new PlantPoint(index, new Vector3(xPos, 0, y * 2) + _cropsParent.position);
            }
        }
    }
    #region ForFarmer
    public PlantPoint GetPlantPointById(int id)
    {
        return _plantPoints[id];
    }
    public PlantPoint GetPlantPoint(int startIndex, Farmer.Stats stats)
    {
        PlantPoint target = null;
        if (stats == Farmer.Stats.harvest)
            target = GetPlantPointToPlant(true, startIndex, _plantPoints.Length);
        else if (stats == Farmer.Stats.plant)
            target = GetPlantPointToPlant(false, startIndex, _plantPoints.Length);
        else
        {
            target = GetPlantPointToPlant(true, startIndex, _plantPoints.Length);
            if (target == null)
                target = GetPlantPointToPlant(false, startIndex, _plantPoints.Length);
        }
        return target;
    }
    private PlantPoint GetPlantPointToPlant(bool targetHarvest, int startIndex, int endIndex)
    {
        PlantPoint plant = null;
        for (int i = startIndex; i < endIndex; i++)
        {
            if (_plantPoints[i].targetted) continue;

            if (targetHarvest)
            {
                if (_plantPoints[i].crops == null || !_plantPoints[i].crops.CanBeHarvest()) continue;
            }
            else if (_plantPoints[i].crops != null) continue;

            plant = _plantPoints[i];
            break;
        }
        if (plant == null && startIndex != 0)
        {
            plant = GetPlantPointToPlant(targetHarvest, 0, startIndex);
        }
        return plant;
    }
    public void InitPlant(PlantPoint plantPoint, CropsData data)
    {
        plantPoint.crops = Instantiate(data.obj, plantPoint.position, Quaternion.identity, _cropsParent);
        plantPoint.crops.Init(data);
    }
    public int[] FindBestCorp()
    {
        int id = -1;
        int food = -1;

        for (int i = 0; i < _plantPoints.Length; i++)
        {
            if (_plantPoints[i].crops != null && _plantPoints[i].crops.CanBeHarvest() && _plantPoints[i].crops.GetFoodValue() > food)
            {
                id = i;
                food = _plantPoints[i].crops.GetFoodValue();
            }
        }

        return new int[] { id, food};
    }
    #endregion
    #endregion

    [System.Serializable]
    public class PlantPoint
    {
        public int index;
        public Vector3 position;
        public Crops crops;
        public bool targetted;

        public PlantPoint(int index, Vector3 position)
        {
            this.index = index;
            this.position = position;
            crops = null;
            targetted = false;
        }
    }
}
