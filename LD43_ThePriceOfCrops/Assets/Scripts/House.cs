using System.Collections;
using UnityEngine;

public class House : MonoBehaviour
{
    #region Vars
    [SerializeField] private float _reproduceTime = 15;

    private Habitant[] _habitants = new Habitant[2];

    public Transform door, inside = null;
    #endregion
    #region Functions
    public bool AddFarmer(Farmer farmer)
    {
        if (_habitants[0].farmer == null)
            _habitants[0].farmer = farmer;
        else if (_habitants[1].farmer == null)
            _habitants[1].farmer = farmer;
        else return false;

        farmer.SendToHouse(this);
        return true;
    }
    public void FarmerArrived(Farmer farmer)
    {
        if (farmer == _habitants[0].farmer)
            _habitants[0].inside = true;
        else if (farmer == _habitants[1].farmer)
            _habitants[1].inside = true;

        //PlaceFarmer + Anim
        if (_habitants[0].inside && _habitants[1].inside)
        {
            _habitants[0].farmer.OnlyMouseToPoint(inside.position);
            _habitants[1].farmer.OnlyMouseToPoint(inside.position);
            StartCoroutine(ReproduceTimer());
        }
    }
    private IEnumerator ReproduceTimer()
    {
        yield return new WaitForSeconds(_reproduceTime);
        Reproduce();
    }
    private void Reproduce()
    {
        Debug.Log("Reproduce");

        if (_habitants[0].farmer != null)
        {
            _habitants[0].farmer.LeaveHouse();
            _habitants[0].Clear();
        }
        if(_habitants[1].farmer != null)
        {
            _habitants[1].farmer.LeaveHouse();
            _habitants[1].Clear();
        }


        Farmer f = GameManager.inst.InstatiateFarmer(door.position, door.rotation);
        f.SetDestination(GameManager.inst.villageCenter.position);
    }
    #endregion

    private struct Habitant
    {
        public Farmer farmer;
        public bool inside;

        public void Clear()
        {
            farmer = null;
            inside = false;
        }
    }
}
