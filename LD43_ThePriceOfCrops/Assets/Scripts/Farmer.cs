using UnityEngine;
using UnityEngine.AI;

public class Farmer : MonoBehaviour {

    public enum Stats { idle, plant, harvest }

    #region Vars
    public Stats stats;

    private Field _field;
    private Field.PlantPoint _plantPoint;
    private int _crtFielIndex = -1;
    private bool _needplantPoint = true, _needToPlant = false;
    private float _plantTime = -1;
    private CropsData _crtPlantingCropsData;

    private const float _harvestDst = 0.4f;
    private const float _storeCropsDst = 0.2f;

    private Crops _draggedCrops = null;

    [SerializeField] private Transform _dragPosition = null;

    private NavMeshAgent _agent;
    private Transform _transform;
    #endregion
    #region MonoFunctions
    private void Start ()
    {
        _agent = GetComponent<NavMeshAgent>();
        _transform = GetComponent<Transform>();
    }
	
	private void Update ()
    {
        if (_needToPlant)
        {
            if (_plantTime < Time.time)
                Plant();
        }
        else if (_draggedCrops != null)
        {
            if (Vector3.Distance(_transform.position, GameManager.inst.grenary.position) < _storeCropsDst) //is at destination
            {
                GameManager.inst.foodStored += _draggedCrops.GetFoodValue();
                Destroy(_draggedCrops.gameObject);
                _draggedCrops = null;
            }
        }
        else if (_field != null)
            FieldManagement();
        else if (stats != Stats.idle)
            stats = Stats.idle;
    }
    #endregion
    #region Functions
    public bool CanBeSelected()
    {
        bool canBeSelected = true;
        if (_needToPlant || _draggedCrops != null)
            canBeSelected = false;
        return canBeSelected;
    }
    public void SetDestination(Vector3 dst)
    {
        stats = Stats.idle;
        _field = null;
        _agent.SetDestination(dst);
    }
    public void SetOccupation(Field field)
    {
        _field = field;
    }
    private void FieldManagement()
    {
        if (_needplantPoint)
        {
            if (Options.autoPlantAfterHarvest)
                _plantPoint = _field.GetPlantPoint(_crtFielIndex, Stats.idle);
            else _plantPoint = _field.GetPlantPoint(_crtFielIndex + 1, stats);
        }
        if (_plantPoint == null)
        {
            _field = null;
            //set destination to Village center
            _agent.SetDestination(GameManager.inst.villageCenter.position);
            stats = Stats.idle;
            _crtFielIndex = -1;
        }
        else
        {
            _plantPoint.targetted = true;
            _needplantPoint = false;
            _crtFielIndex = _plantPoint.index;
            _agent.SetDestination(_plantPoint.position);
            if (Vector3.Distance(_transform.position, _plantPoint.position) < _harvestDst) //is at destination
            {
                if (_plantPoint.crops == null)
                {
                    _needToPlant = true;
                    _crtPlantingCropsData = _field.cropsData;
                    _plantTime = Time.time + _crtPlantingCropsData.plantTime;
                }
                else
                {
                    stats = Stats.harvest;
                    _draggedCrops = _plantPoint.crops;
                    _draggedCrops._transform.parent = _transform;
                    _draggedCrops._transform.position = _dragPosition.position;
                    _draggedCrops._transform.rotation = Quaternion.identity;
                    _plantPoint.crops = null;
                    _plantPoint.targetted = false;
                    _plantPoint = null;
                    _needplantPoint = true;
                    _agent.SetDestination(GameManager.inst.grenary.position);
                }
            }
        }
    }
    private void Plant()
    {
        stats = Stats.plant;
        _field.InitPlant(_plantPoint, _crtPlantingCropsData);
        _plantPoint.targetted = false;
        _plantPoint = null;
        _needplantPoint = true;
        _needToPlant = false;
    }
    #endregion

}
