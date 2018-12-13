using UnityEngine;
using UnityEngine.AI;

public class Farmer : MonoBehaviour {

    public enum Stats { idle, move, plant, harvest, sacrifice, goToEat, goToHouse, isBacker }

    #region Vars
    public Stats state;
    private Stats _previousStats;

    private Vector3 _targetPosition;
    private bool _foodSendToSacrifice;

    public bool CanBeSelectIt = true;

    private Field _field;
    private Field.PlantPoint _plantPoint;
    private int _crtPlantPointIndex = -1;
    private bool _needplantPoint = true, _needToPlant = false;
    private float _plantTime = -1;
    private CropsData _crtPlantingCropsData;

    private const float _harvestDst = 1.1f;
    private const float _storeCropsDst = 1.5f;

    private Crops _draggedCrops = null;

    [SerializeField] private int wannaEatEachAtStart = 50; // in sec
    internal float wannaEatTime;
    private int[] _wannaEatTargetIds;
    private bool _eatAgain = false;

    [SerializeField] private Transform _dragPosition = null;
    [SerializeField] private int maxEat = 180;

    private bool _isAtHouse;
    private House _targettedHouse;

    [SerializeField] private Animator _animator;
    private NavMeshAgent _agent;
    private Transform _transform;

    public GameObject selected;
    [SerializeField] private StateIcon _stateIcon; 
    #endregion
    #region MonoFunctions
    private void Start ()
    {
        _agent = GetComponent<NavMeshAgent>();
        _transform = GetComponent<Transform>();

        wannaEatTime = GameManager.time + wannaEatEachAtStart;

        int minPriority = 30, maxPriority = 60;
        _agent.avoidancePriority = Random.Range(minPriority, maxPriority);
    }
	private void Update ()
    {
        if (state == Stats.isBacker)
        {

        }
        else if (_needToPlant) //waiting 
        {
            if (_plantTime > 0 && _plantTime < GameManager.time)
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
        else if (state == Stats.goToHouse)
        {
            if (!_isAtHouse && Vector3.Distance(_transform.position, _targettedHouse.door.position) < _storeCropsDst)
            {
                _isAtHouse = true;
                _targettedHouse.FarmerArrived(this);
            }
        }
        else if (wannaEatTime < GameManager.time || _eatAgain)
        {
            if (state == Stats.goToEat)
            {
                if (_wannaEatTargetIds[0] == -1)
                {
                    if (Vector3.Distance(_transform.position, GameManager.inst.grenary.position) < _storeCropsDst)
                    {
                        //Add Anim eat + sound

                        _eatAgain = wannaEatTime < GameManager.time;
                        int foodNutrition = Grenary.inst.GetClaimedFood(_wannaEatTargetIds[1]);
                        if (wannaEatTime - GameManager.time < 0)
                            wannaEatTime = 0;
                        wannaEatTime += GameManager.time + foodNutrition;

                        ResetPreviousStatsAfterEat(!_eatAgain);
                        _stateIcon.UpdateState(StateIcon.States.none);
                    }
                }
                else
                {
                    if (_plantPoint != null && Vector3.Distance(_transform.position, _plantPoint.position) < _harvestDst)
                    {
                        //Add Anim eat + sound
                        _eatAgain = wannaEatTime < GameManager.time;
                        int foodNutrition = _plantPoint.crops.GetFoodValue();

                        if (wannaEatTime - GameManager.time < 0)
                            wannaEatTime = 0;
                        wannaEatTime += GameManager.time + foodNutrition;

                        Destroy(_plantPoint.crops.gameObject);
                        _plantPoint.crops = null;
                        _plantPoint.targetted = false;
                        _plantPoint = null;
                        _needplantPoint = true;
                        //_crtPLantPointIndex = -1;

                        ResetPreviousStatsAfterEat(!_eatAgain);

                        _stateIcon.UpdateState(StateIcon.States.none);
                    }
                }

                //If low in food try to find more
            }
            else
            {
                if (_plantPoint != null)
                {
                    _plantPoint.targetted = false;
                    _plantPoint = null;
                }

                //Find nearestFood and better to eat
                _wannaEatTargetIds = FoodManager.inst.FindFood();
                if (_wannaEatTargetIds[0] == -1 && _wannaEatTargetIds[1] == -1)
                {
                    if (wannaEatTime < GameManager.time)
                    {
                        Debug.Log("No food found and die");
                        Kill();
                    }
                    else
                    {
                        _eatAgain = false;
                        ResetPreviousStatsAfterEat(true);
                        Debug.Log("No food found");
                    }
                }
                else
                {
                    _stateIcon.UpdateState(StateIcon.States.eat);

                    _previousStats = state;
                    state = Stats.goToEat;
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
        else if (state == Stats.sacrifice)
        {
            if (Vector3.Distance(_transform.position, GameManager.inst.sacrificePlace.position) < _storeCropsDst) //is at destination
                Sacrifice.inst.SacrificeFarmer(this);
        }
        else if (state == Stats.idle)
        {
            if (Vector3.Distance(_transform.position, GameManager.inst.villageCenter.position) < 3) //is at destination
                _agent.SetDestination(_transform.position);
        }
        /*else if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Planting") && !_startedPlanting)
        {
            AnimTriggerEndPlant();
            Debug.Log("ForceKill : Planting");
        }*/
        AnimSetSpeed(_agent.velocity.magnitude);
    }
    #endregion
    #region Functions
    public bool CanBeSelected()
    {
        bool canBeSelected = true;
        if (!CanBeSelectIt || _needToPlant || _draggedCrops != null || state == Stats.goToEat || _startedHarvesting || _startedPlanting)
            canBeSelected = false;
        return canBeSelected;
    }
    public void Selected(bool displaySelect = true)
    {
        if (state == Stats.isBacker)
            Backery.inst.SetFarmer(null);

        if (state == Stats.goToHouse)
        {
            if (_isAtHouse)
                _isAtHouse = false;
            _targettedHouse.FarmerLeaved(this);
        }

        if (displaySelect)
            selected.SetActive(true);
        state = Stats.idle;
        //_needToPlant = false;
        _field = null;
        _crtPlantPointIndex = -1;
        if (_plantPoint != null)
        {
            _plantPoint.targetted = false;
            _plantPoint = null;
        }
        _agent.SetDestination(_transform.position);

        _stateIcon.UpdateState(StateIcon.States.none);
    }
    public void SetDestination(Vector3 dst)
    {
        if (_needToPlant) return;

        state = Stats.move;
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
        state = Stats.sacrifice;
        _needToPlant = false;
        _field = null;
        if (_plantPoint != null)
            _plantPoint.targetted = false;
        _agent.SetDestination(GameManager.inst.sacrificePlace.position);

        _stateIcon.UpdateState(StateIcon.States.sacrifice);
    }
    public void SendToEat()
    {
        if (wannaEatTime - GameManager.time < maxEat)
            _eatAgain = true;
        else Debug.Log("eat to mush already");
    }
    public void SendToHouse(House house)
    {
        state = Stats.goToHouse;
        _needToPlant = false;
        _field = null;
        if (_plantPoint != null)
            _plantPoint.targetted = false;

        _targettedHouse = house;
        _agent.SetDestination(house.door.position);

        _stateIcon.UpdateState(StateIcon.States.reproduction);
    }
    public void LeaveHouse()
    {
        //_targettedHouse = null;
        state = Stats.idle;
        CanBeSelectIt = true;
        _agent.SetDestination(GameManager.inst.villageCenter.position);

        _stateIcon.UpdateState(StateIcon.States.none);
    }
    public void OnlyMouseToPoint(Vector3 position)
    {
        _agent.SetDestination(position);
    }
    private void FieldManagement()
    {
        if (_startedHarvesting || _startedPlanting)
            return;

        if (_needplantPoint)
        {
            if (Options.autoPlantAfterHarvest)
                _plantPoint = _field.GetPlantPoint(_crtPlantPointIndex, Stats.idle);
            else _plantPoint = _field.GetPlantPoint(_crtPlantPointIndex + 1, state);
        }
        if (_plantPoint == null)
        {
            _field = null;
            //set destination to Village center
            _agent.SetDestination(GameManager.inst.villageCenter.position);
            state = Stats.idle;
            _crtPlantPointIndex = -1;
        }
        else
        {
            _plantPoint.targetted = true;
            _needplantPoint = false;
            _crtPlantPointIndex = _plantPoint.index;
            _agent.SetDestination(_plantPoint.position);

            if (_plantPoint.crops == null)
                _stateIcon.UpdateState(StateIcon.States.plant);
            else if (_plantPoint.crops.eatable)
                _stateIcon.UpdateState(StateIcon.States.storing);
            else _stateIcon.UpdateState(StateIcon.States.trash);

            if (Vector3.Distance(_transform.position, _plantPoint.position) < _harvestDst) //is at destination
            {
                _agent.SetDestination(_transform.position);

                if (_plantPoint.crops == null)
                {
                    state = Stats.plant;
                    _startedPlanting = true;
                    _needToPlant = true;
                    _plantTime = -1;
                    _crtPlantingCropsData = _field.cropsData;
                    AnimTriggerPlant();
                }
                else
                {
                    state = Stats.harvest;
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

        _stateIcon.UpdateState(StateIcon.States.none);
    }
    public void Planting()
    {
        _plantTime = GameManager.time + _crtPlantingCropsData.plantTime;
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
        state = _previousStats;
        if (state == Stats.idle && move)
            _agent.SetDestination(GameManager.inst.villageCenter.position);
    }
    public void Kill()
    {
        if (_plantPoint != null)
            _plantPoint.targetted = false;

        this.enabled = false;
        _agent.enabled = false;
        GetComponent<Collider>().enabled = false;

        GameManager.inst.RemoveFarmer(this);

        AnimTriggerDeath();
    }
    public void Death()
    {
        Destroy(gameObject);
    }
    public void DragObj(Crops c)
    {
        c.Drag(_dragPosition);
    }
    #endregion
    #region AnimationFunction
    private bool _startedHarvesting, _startedPlanting = false;

    public void AnimTriggerCarry()
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
    public void AnimTriggerDeath()
    {
        _animator.SetTrigger("Death");
    }
    #endregion
}
