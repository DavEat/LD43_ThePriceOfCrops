using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Backery : MonoBehaviour
{
    public static Backery inst;

    private enum CookingState { come, get, comeback, prepare, cook, bring }

    #region Vars
    private Farmer _farmer;
    private CookingState _state;

    [SerializeField] private Transform _door, _inside = null, _oven = null;

    private float _interactDst = 1.2f;
    private bool _stuffReserved;

    private float /*_prepareTime = 5f,*/ _cookTime = 5f;
    private float _time = -1;

    private bool _bringToGod = false;
    private Vector3 _destination;

    [SerializeField] private Crops _generateCropsPrefab;
    [SerializeField] private Crops _generateCrops;
    [SerializeField] private Crops _breadIngredient;
    #endregion
    #region MonoFunctions
    private void Awake()
    {
        inst = this;
    }
    private void Start()
    {
        _generateCrops.Init(FoodManager.inst.GetCrops(3));
        _breadIngredient.Init(FoodManager.inst.GetCrops(0));
    }
    private void Update()
    {
        if (_farmer == null) return;

        if (_state == CookingState.come)
        {
            if (Vector3.Distance(_farmer.transform.position, _door.position) < _interactDst)
            {
                //try find cooking stuff in grenary
                _stuffReserved = Grenary.inst.FindCookingStuff();
                if (_stuffReserved)
                {
                    _state = CookingState.get;
                    _farmer.OnlyMouseToPoint(GameManager.inst.grenary.position);
                }
                else Debug.Log("wait for stuff");
            }
        }
        else if (_state == CookingState.get)
        {
            if (Vector3.Distance(_farmer.transform.position, GameManager.inst.grenary.position) < _interactDst)
            {
                Grenary.inst.GetCookingStuff();
                _state = CookingState.comeback;
                _farmer.OnlyMouseToPoint(_inside.position);

                _farmer.DragObj(_breadIngredient);
                _farmer.AnimTriggerCarry();
            }
        }
        else if (_state == CookingState.comeback)
        {
            if (Vector3.Distance(_farmer.transform.position, _inside.position) < _interactDst)
            {
                _state = CookingState.cook;
                _farmer.OnlyMouseToPoint(_oven.position);
            }
        }
        else if (_state == CookingState.cook)
        {
            if (Vector3.Distance(_farmer.transform.position, _oven.position) < _interactDst)
            {
                if (_time == -1)
                {
                    _breadIngredient.Drag(_inside);
                    _farmer.AnimTriggerStopCarry();
                    _time = GameManager.time + _cookTime;
                }
                else if (_time < GameManager.time)
                {
                    _time = -1;
                    _state = CookingState.bring;
                    if (_bringToGod = GameManager.inst.GetHarvestFoodDestination())
                        _destination = (GameManager.inst.sacrificePlace.position);
                    else _destination = (GameManager.inst.grenary.position);
                    _farmer.OnlyMouseToPoint(_destination);

                    _farmer.DragObj(_generateCrops);
                    _farmer.AnimTriggerCarry();
                }
            }
        }
        else if (_state == CookingState.bring)
        {
            if (Vector3.Distance(_farmer.transform.position, _destination) < 1.2f)
            {
                if (_bringToGod)
                {
                    _farmer.OnlyMouseToPoint(_farmer.transform.position);
                    Sacrifice.inst.SacrificeFood(_generateCrops);
                }
                else Grenary.inst.AddFood(_generateCrops);

                _generateCrops.Drag(_inside);

                _stuffReserved = Grenary.inst.FindCookingStuff();
                if (_stuffReserved)
                {
                    _state = CookingState.get;
                    _farmer.OnlyMouseToPoint(GameManager.inst.grenary.position);
                }
                else
                {
                    _state = CookingState.come;
                    _farmer.OnlyMouseToPoint(_door.position);
                    _farmer.AnimTriggerStopCarry();
                }
            }
        }
    }
    #endregion
    #region Functions
    internal void SetFarmer(Farmer farmer)
    {
        if (_farmer != null)
        {
            if (farmer != null) return;
            if (_state == CookingState.comeback || _state == CookingState.prepare || _state == CookingState.cook || _breadIngredient._transform.parent != _inside)
                _breadIngredient._transform.parent = _inside;
        }

        if (farmer == null)
            _farmer = null;
        else
        {
            _farmer = farmer;
            if (_farmer.CanBeSelected())
            {
                _farmer.state = Farmer.Stats.isBacker;
                _state = CookingState.come;

                _farmer.OnlyMouseToPoint(_door.position);
            }
        }
    }
    #endregion
}