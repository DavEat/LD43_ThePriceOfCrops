using UnityEngine;
using UnityEngine.AI;

public class Farmer : MonoBehaviour {

    public enum Stats { idle, plant, harvest }

    #region Vars
    public Stats stats;

    private Field _field;
    private Field.PlantPoint _plantPoint;
    private bool needplantPoint = true;

    private const float _harvestDst = 0.4f;
    private const float _storeCropsDst = 0.2f;

    private Crops _draggedCrops = null;

    [SerializeField] private Transform _dragPosition;

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
        if (_draggedCrops != null)
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
    }
    #endregion
    #region Functions
    public void SetDestination(Vector3 dst)
    {
        _agent.SetDestination(dst);
    }
    public void SetOccupation(Field field)
    {
        _field = field;
    }

    private void FieldManagement()
    {
        if (needplantPoint)
        {
            if (Options.autoPlantAfterHarvest)
                _plantPoint = _field.GetPlantPoint();
            else _plantPoint = _field.GetPlantPoint(stats);
        }
        if (_plantPoint == null)
        {
            _field = null;
            //set destination to Village center
            _agent.SetDestination(GameManager.inst.villageCenter.position);
            stats = Stats.idle;
        }
        else
        {
            needplantPoint = false;
            _agent.SetDestination(_plantPoint.position);
            if (Vector3.Distance(_transform.position, _plantPoint.position) < _harvestDst) //is at destination
            {
                if (_plantPoint.crops == null)
                {
                    stats = Stats.plant;
                    _field.InitPlant(_plantPoint);
                    _plantPoint = null;
                    needplantPoint = true;
                }
                else
                {
                    stats = Stats.harvest;
                    _draggedCrops = _plantPoint.crops;
                    _draggedCrops._transform.parent = _transform;
                    _draggedCrops._transform.position = _dragPosition.position;
                    _draggedCrops._transform.rotation = Quaternion.identity;
                    _plantPoint.crops = null;
                    _plantPoint = null;
                    needplantPoint = true;
                    _agent.SetDestination(GameManager.inst.grenary.position);
                }
            }
        }
    }
    #endregion

}
