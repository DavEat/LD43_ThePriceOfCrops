using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour {

    #region Vars
    [SerializeField] private Transform _cropsParent;
    [SerializeField] private Vector2Int _fieldSize = new Vector2Int(5, 5);

    [HideInInspector] public CropsData cropsData;

    private PlantPoint[] plantPoints;

    [SerializeField] private GameObject _interface;
    #endregion
    #region MonoFunctions
    private void Start ()
    {
        InitPlantPoint();
        cropsData = CropsManager.inst.GetCrops(0);
    }
	private void Update ()
    {
		
	}
    #endregion
    #region Functions
    public void ShowInterface(bool value)
    {
        _interface.SetActive(value);
    }
    public void SelectCrops(int id)
    {
        cropsData = CropsManager.inst.GetCrops(id);
    }
    private void InitPlantPoint()
    {
        plantPoints = new PlantPoint[_fieldSize.x * _fieldSize.y];
        for (int y = 0; y < _fieldSize.y; y++)
        {
            for (int x = 0; x < _fieldSize.x; x++)
            {
                int xPos = (y % 2 == 0 ? (x * 2) : ((_fieldSize.x - 1) - x) * 2);
                plantPoints[y * _fieldSize.x + x] = new PlantPoint(new Vector3(xPos, 0, y * 2) + _cropsParent.position);
            }
        }
    }
    #region ForFarmer
    public PlantPoint GetPlantPoint(Farmer.Stats stats = Farmer.Stats.idle)
    {
        PlantPoint harvest = null, plant = null;
        for (int i = 0; i < plantPoints.Length; i++)
        {
            if (plantPoints[i].crops != null)
            {
                if (plantPoints[i].crops.CanHarvest() && stats != Farmer.Stats.plant)
                {
                    harvest = plantPoints[i];
                    break;
                }
            }
            else if (plant == null && stats != Farmer.Stats.harvest)
                plant = plantPoints[i];
        }
        return harvest == null ? plant : harvest;
    }
    public void InitPlant(PlantPoint plantPoint)
    {
        plantPoint.crops = Instantiate(cropsData.obj, plantPoint.position, Quaternion.identity, _cropsParent);
        plantPoint.crops.Init(cropsData);
    }
    #endregion
    #endregion

    [System.Serializable]
    public class PlantPoint
    {
        public Vector3 position;
        public Crops crops;

        public PlantPoint(Vector3 position)
        {
            this.position = position;
            crops = null;
        }
    }
}
