﻿using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager inst;

    #region Vars
    private Camera _camera;

    private Vector2 _mouseDownPos;

    private int _rayMaxDist = 100;
    [SerializeField] private LayerMask _layerMask;

    private Farmer _selectedFarmer;
    private Field _field;

    public Transform villageCenter, grenary = null, sacrificePlace = null;
    #endregion
    #region MonoFunctions
    private void Awake()
    {
        inst = this;
    }
    private void Start ()
    {
        _camera = Camera.main;
    }
	private void Update ()
    {
        if (Input.GetMouseButtonDown(0))
        //    _mouseDownPos = Input.mousePosition;
        //if (Input.GetMouseButtonUp(0) && Vector2.Distance(_mouseDownPos, Input.mousePosition) < 1.5f)
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, _rayMaxDist, _layerMask))
            {
                HitSelection(ref hit);
            }
            Debug.DrawRay(ray.origin, hit.point, Color.yellow);
        }
    }
    #endregion
    #region Functions
    private void HitSelection(ref RaycastHit hit)
    {
        if (_field != null && !hit.collider.CompareTag("Field"))
        {
            _field.ToggleInterface(false);
            _field = null;
        }

        //Debug.Log
        switch (hit.collider.tag)
        {
            case "Farmer":
                FarmerClicked(hit.transform.GetComponent<Farmer>());
                break;
            case "Ground":
                if (_selectedFarmer != null)
                    MoveSelectedFarmer(hit.point);
                break;
            case "Field":
                Field f = hit.transform.GetComponent<Field>();
                if (_selectedFarmer != null)
                {
                    _selectedFarmer.SendToField(f);
                    _selectedFarmer = null;
                }
                else
                {
                    _field = f;
                    f.ToggleInterface(true);
                }
                break;
            case "Interface":
                if (_selectedFarmer == null)
                    hit.transform.GetComponent<WorldButton>().OnClick();
                break;
            case "Sacrifice":
                if (_selectedFarmer != null)
                {
                    _selectedFarmer.SendToSacrifice();
                    _selectedFarmer = null;
                }
                break;
            case "Grenary":
                if (_selectedFarmer != null)
                {
                    _selectedFarmer.SendToEat();
                    _selectedFarmer = null;
                }
                break;
            default:
                Debug.LogErrorFormat("No tag found for this collider : {0}", hit.collider.name);
                break;
        }
    }
    private void FarmerClicked(Farmer farmer)
    {        
        if (farmer.CanBeSelected() && _selectedFarmer == null)
        {
            _selectedFarmer = farmer;
            _selectedFarmer.Selected();
        }
    }
    private void MoveSelectedFarmer(Vector3 to)
    {
        _selectedFarmer.SetDestination(to);
        if (Options.unselectFarmerOnDestination)
            _selectedFarmer = null;
    }
    #endregion
}