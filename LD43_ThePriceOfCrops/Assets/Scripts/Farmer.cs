using UnityEngine;
using UnityEngine.AI;

public class Farmer : MonoBehaviour {

    public enum Stats { idle, move, plant, harvest, sacrifice, goToEat }

    #region Vars
    public Stats stats;
    private Stats _previousStats;

    private Field _field;
    private Field.PlantPoint _plantPoint;
    private int _crtPLantPointIndex = -1;
    private bool _needplantPoint = true, _needToPlant = false;
    private float _plantTime = -1;
    private CropsData _crtPlantingCropsData;

    private const float _harvestDst = 0.4f;
    private const float _storeCropsDst = 1.2f;

    private Crops _draggedCrops = null;

    [SerializeField] private int wannaEatEachAtStart = 50; // in sec
    private float wannaEatTime;
    private int[] _wannaEatTargetIds;
    private bool _eatAgain = false;

    [SerializeField] private Transform _dragPosition = null;
    [SerializeField] private int maxEat = 180;

    private NavMeshAgent _agent;
    private Transform _transform;
    #endregion
    #region MonoFunctions
    private void Start ()
    {
        _agent = GetComponent<NavMeshAgent>();
        _transform = GetComponent<Transform>();

        wannaEatTime = Time.time + wannaEatEachAtStart;
    }
	
	private void Update ()
    {
        if (_needToPlant) //waiting 
        {
            if (_plantTime < Time.time)
                Plant();
        }
        else if (_draggedCrops != null) //waiting 
        {
            if (Vector3.Distance(_transform.position, GameManager.inst.grenary.position) < _storeCropsDst) //is at destination
            {
                Grenary.inst.AddFood(_draggedCrops);
                Destroy(_draggedCrops.gameObject);
                _draggedCrops = null;
            }
        }
        else if (wannaEatTime < Time.time || _eatAgain)
        {
            if (stats == Stats.goToEat)
            {
                if (_wannaEatTargetIds[0] == -1)
                {
                    if (Vector3.Distance(_transform.position, GameManager.inst.grenary.position) < _storeCropsDst)
                    {
                        //Add Anim eat + sound

                        _eatAgain = wannaEatTime < Time.time;
                        int foodNutrition = Grenary.inst.GetClaimedFood(_wannaEatTargetIds[1]);
                        if (wannaEatTime - Time.time < 0)
                            wannaEatTime = 0;
                        wannaEatTime += Time.time + foodNutrition;

                        ResetPreviousStatsAfterEat(!_eatAgain);
                    }
                }
                else
                {
                    if (Vector3.Distance(_transform.position, _plantPoint.position) < _harvestDst)
                    {
                        //Add Anim eat + sound
                        _eatAgain = wannaEatTime < Time.time;
                        int foodNutrition = _plantPoint.crops.GetFoodValue();

                        if (wannaEatTime - Time.time < 0)
                            wannaEatTime = 0;
                        wannaEatTime += Time.time + foodNutrition;

                        Destroy(_plantPoint.crops.gameObject);
                        _plantPoint.crops = null;
                        _plantPoint.targetted = false;
                        _plantPoint = null;
                        _needplantPoint = true;
                        //_crtPLantPointIndex = -1;

                        ResetPreviousStatsAfterEat(!_eatAgain);
                    }
                }
                
                //If low in food try to find more
            }
            else
            {
                //Find nearestFood and better to eat
                _wannaEatTargetIds = FoodManager.inst.FindFood();
                if (_wannaEatTargetIds[0] == -1 && _wannaEatTargetIds[1] == -1)
                {
                    if (wannaEatTime < Time.time)
                    {
                        Debug.Log("No food found and die");
                        this.enabled = false;
                        Destroy(gameObject);
                    }
                    _eatAgain = false;
                    Debug.Log("No food found");
                }
                else
                {
                    _previousStats = stats;
                    stats = Stats.goToEat;
                    if (_wannaEatTargetIds[0] == -1) //grenary
                        _agent.SetDestination(GameManager.inst.grenary.position);
                    else //field
                    {
                        _plantPoint = FoodManager.inst.GetFieldPlantPoint(_wannaEatTargetIds);
                        _plantPoint.targetted = true;
                        _agent.SetDestination(_plantPoint.position);
                    }
                }
            }
        }
        else if (_field != null)
            FieldManagement();
        else if (stats == Stats.sacrifice)
        {
            if (Vector3.Distance(_transform.position, GameManager.inst.sacrificePlace.position) < _storeCropsDst) //is at destination
                Sacrifice.inst.SacrificeFarmer(this);
        }
        //else if (stats != Stats.idle)
        //    stats = Stats.idle;
    }
    #endregion
    #region Functions
    public bool CanBeSelected()
    {
        bool canBeSelected = true;
        if (_needToPlant || _draggedCrops != null || stats == Stats.goToEat)
            canBeSelected = false;
        return canBeSelected;
    }
    public void Selected()
    {
        stats = Stats.idle;
        _needToPlant = false;
        _field = null;
        if (_plantPoint != null)
        {
            _plantPoint.targetted = false;
            _plantPoint = null;
        }
        _agent.SetDestination(_transform.position);
    }
    public void SetDestination(Vector3 dst)
    {
        if (_needToPlant) return;

        stats = Stats.move;
        _field = null;
        if (_plantPoint != null)
            _plantPoint.targetted = false;
        _agent.SetDestination(dst);
    }
    public void SendToField(Field field)
    {
        if (_needToPlant) return;

        if (_plantPoint != null)
            _plantPoint.targetted = false;

        _needplantPoint = true;
        _field = field;
    }
    public void SendToSacrifice()
    {
        if (_needToPlant) return;

        stats = Stats.sacrifice;
        _needToPlant = false;
        _field = null;
        if (_plantPoint != null)
            _plantPoint.targetted = false;
        _agent.SetDestination(GameManager.inst.sacrificePlace.position);
    }
    public void SendToEat()
    {
        if (wannaEatTime - Time.time < maxEat)
            _eatAgain = true;
        else Debug.Log("eat to mush already");
    }
    private void FieldManagement()
    {
        if (_needplantPoint)
        {
            if (Options.autoPlantAfterHarvest)
                _plantPoint = _field.GetPlantPoint(_crtPLantPointIndex, Stats.idle);
            else _plantPoint = _field.GetPlantPoint(_crtPLantPointIndex + 1, stats);
        }
        if (_plantPoint == null)
        {
            _field = null;
            //set destination to Village center
            _agent.SetDestination(GameManager.inst.villageCenter.position);
            stats = Stats.idle;
            _crtPLantPointIndex = -1;
        }
        else
        {
            _plantPoint.targetted = true;
            _needplantPoint = false;
            _crtPLantPointIndex = _plantPoint.index;
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
    private void ResetPreviousStatsAfterEat(bool move)
    {
        stats = _previousStats;
        if (stats == Stats.idle && move)
            _agent.SetDestination(GameManager.inst.villageCenter.position);
    }
    #endregion

}
