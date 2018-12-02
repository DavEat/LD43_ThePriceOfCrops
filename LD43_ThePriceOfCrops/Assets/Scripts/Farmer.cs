using UnityEngine;
using UnityEngine.AI;

public class Farmer : MonoBehaviour {

    public enum Stats { idle, move, plant, harvest, sacrifice, goToEat, goToHouse }

    #region Vars
    public Stats stats;
    private Stats _previousStats;

    private Vector3 _targetPosition;
    private bool _foodSendToSacrifice;

    private Field _field;
    private Field.PlantPoint _plantPoint;
    private int _crtPLantPointIndex = -1;
    private bool _needplantPoint = true, _needToPlant = false;
    private float _plantTime = -1;
    private CropsData _crtPlantingCropsData;

    private const float _harvestDst = 1.1f;
    private const float _storeCropsDst = 1.5f;

    private Crops _draggedCrops = null;

    [SerializeField] private int wannaEatEachAtStart = 50; // in sec
    private float wannaEatTime;
    private int[] _wannaEatTargetIds;
    private bool _eatAgain = false;

    [SerializeField] private Transform _dragPosition = null;
    [SerializeField] private int maxEat = 180;

    private House _targettedHouse;

    [SerializeField] private Animator _animator;
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
            if (_plantTime > 0 && _plantTime < Time.time)
            {
                Plant();
                _startedPlanting = false;
                AnimTriggerEndPlant();
            }
        }
        else if (_draggedCrops != null) //waiting 
        {
            if (Vector3.Distance(_transform.position, _targetPosition) < _storeCropsDst) //is at destination
            {
                AnimTriggerStopCarry();
                if (_draggedCrops.eatable)
                {
                    if (_foodSendToSacrifice)
                    {
                        _agent.SetDestination(_transform.position);
                        Sacrifice.inst.SacrificeFood(_draggedCrops);
                    }
                    else Grenary.inst.AddFood(_draggedCrops);
                }
                Destroy(_draggedCrops.gameObject);
                _draggedCrops = null;
            }
        }
        else if (stats == Stats.goToHouse)
        {
            if (_targettedHouse != null && Vector3.Distance(_transform.position, _targettedHouse.door.position) < _storeCropsDst)
            {
                _targettedHouse.FarmerArrived(this);
                _targettedHouse = null;
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
                    if (_plantPoint != null && Vector3.Distance(_transform.position, _plantPoint.position) < _harvestDst)
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
        else if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Planting"))
        {
            AnimTriggerEndPlant();
            Debug.Log("ForceKill : Planting");
        }
        AnimSetSpeed(_agent.velocity.magnitude);
    }
    #endregion
    #region Functions
    public bool CanBeSelected()
    {
        bool canBeSelected = true;
        if (_needToPlant || _draggedCrops != null || stats == Stats.goToEat || stats == Stats.goToHouse)
            canBeSelected = false;
        return canBeSelected;
    }
    public void Selected()
    {
        stats = Stats.idle;
        //_needToPlant = false;
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

        if (_agent == null)
            _agent = GetComponent<NavMeshAgent>();
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
    public void SendToHouse(House house)
    {
        stats = Stats.goToHouse;
        _needToPlant = false;
        _field = null;
        if (_plantPoint != null)
            _plantPoint.targetted = false;

        _targettedHouse = house;
        _agent.SetDestination(house.door.position);
    }
    public void LeaveHouse()
    {
        //_targettedHouse = null;
        stats = Stats.idle;
        _agent.SetDestination(GameManager.inst.villageCenter.position);
    }
    private void FieldManagement()
    {
        if (_startedHarvesting || _startedPlanting)
            return;

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
                _agent.SetDestination(_transform.position);

                if (_plantPoint.crops == null)
                {
                    stats = Stats.plant;
                    _startedPlanting = true;
                    _needToPlant = true;
                    _plantTime = -1;
                    _crtPlantingCropsData = _field.cropsData;
                    AnimTriggerPlant();
                }
                else
                {
                    stats = Stats.harvest;
                    _startedHarvesting = true;
                    AnimTriggerHarvest();
                }
            }
        }
    }
    private void Harvesting()
    {
        _draggedCrops = _plantPoint.crops;

        _plantPoint.crops.Drag(_dragPosition);
        _plantPoint.crops = null;
        _plantPoint.targetted = false;
        _plantPoint = null;
        _needplantPoint = true;
    }
    private void EndHarvest()
    {
        _startedHarvesting = false;
        if (_draggedCrops.eatable)
        {
            if (_foodSendToSacrifice = GameManager.inst.GetHarvestFoodDestination())
                _targetPosition = GameManager.inst.sacrificePlace.position;
            else _targetPosition = GameManager.inst.grenary.position;
        }
        else _targetPosition = GameManager.inst.trash.position;
        _agent.SetDestination(_targetPosition);
    }
    public void Planting()
    {
        _plantTime = Time.time + _crtPlantingCropsData.plantTime;
    }
    private void Plant()
    {
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
    public void Kill()
    {
        if (_plantPoint != null)
            _plantPoint.targetted = false;

        this.enabled = false;
        GameManager.inst.RemoveFarmer(this);
        Destroy(gameObject);
    }
    #endregion
    #region AnimationFunction
    private bool _startedHarvesting, _startedPlanting = false;

    public void TriggerCarry()
    {
        _animator.SetTrigger("Carry");
    }
    public void AnimTriggerStopCarry()
    {
        _animator.SetTrigger("StopCarry");
    }
    public void AnimTriggerPlant()
    {
        _animator.SetTrigger("Plant");
    }
    public void AnimTriggerEndPlant()
    {
        _animator.SetTrigger("EndPlant");
    }
    public void AnimTriggerHarvest()
    {
        _animator.SetTrigger("Harvest");
    }
    public void AnimSetSpeed(float value)
    {
        _animator.SetFloat("Speed", value);
    }
    #endregion
}
