using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager inst;

    #region Vars
    private Camera _camera;

    private Vector2 _mouseDownPos;

    [SerializeField] private Farmer _farmerPrefab;

    private int _rayMaxDist = 100;
    [SerializeField] private LayerMask _layerMask;

    private Farmer _selectedFarmer;
    private Field _field;

    [SerializeField] private List<Farmer> _farmers = new List<Farmer>();

    public Transform villageCenter, grenary = null, sacrificePlace = null, trash = null;

    private bool _sendFoodToGod;

    public static float deltaTime, time, timeScale = 1;
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
        deltaTime = Time.deltaTime * timeScale;
        time += deltaTime;

        if (timeScale <= 0)
            return;

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
        if (_field != null)
        {
            _field.ToggleInterface(false);
            _field = null;
        }
        if (Sacrifice.inst.InterfaceDisplayed() && !hit.collider.CompareTag("Sacrifice"))
            Sacrifice.inst.ShowInterface(false);

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
                    _selectedFarmer.selected.SetActive(false);
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
                    _selectedFarmer.selected.SetActive(false);
                    _selectedFarmer = null;
                }
                else Sacrifice.inst.ShowInterface(true);
                break;
            case "Grenary":
                if (_selectedFarmer != null)
                {
                    _selectedFarmer.SendToEat();
                    _selectedFarmer.selected.SetActive(false);
                    _selectedFarmer = null;
                }
                break;
            case "House":
                if (_selectedFarmer != null)
                {
                    House h = hit.collider.GetComponent<House>();
                    h.AddFarmer(_selectedFarmer);
                    _selectedFarmer.selected.SetActive(false);
                    _selectedFarmer = null;
                }
                break;
            case "Backery":
                if (_selectedFarmer != null)
                {                    
                    Backery.inst.SetFarmer(_selectedFarmer);
                    _selectedFarmer.selected.SetActive(false);
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
        {
            _selectedFarmer.selected.SetActive(false);
            _selectedFarmer = null;
        }
    }
    public Farmer InstatiateFarmer(Vector3 position, Quaternion rotation, Transform parent = null)
    {
        //if (parent == null) parent = transform.parent;
        Farmer f = Instantiate(_farmerPrefab, position, rotation, parent);
        _farmers.Add(f);
        return f;
    }
    internal void RemoveFarmer(Farmer f)
    {
        _farmers.Remove(f);
        if (_farmers.Count <= 0)
        {
            Debug.Log("GameOver");
            GameManager.timeScale = 0;
            CanvasManager.inst.Reload();
        }
    }
    internal void KillFirstFarmer()
    {
        _farmers[0].Selected(false);
        Lighting.inst.Hit(_farmers[0].transform.position);
        _farmers[0].Kill();
    }
    public void ToggleSendFoodToGod()
    {
        _sendFoodToGod = !_sendFoodToGod;
    }
    internal bool GetHarvestFoodDestination()
    {
        return _sendFoodToGod;
    }
    #endregion
}

