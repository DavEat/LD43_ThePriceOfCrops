using UnityEngine;

public class CropsManager : MonoBehaviour {

    public static CropsManager inst;

    public enum CropsType { pumkin, wheat }

    #region Vars
    [SerializeField] private CropsData[] _crops;
    #endregion
    #region MonoFunctions
    private void Awake()
    {
        inst = this;
    }
    #endregion
    #region Functions
    public CropsData GetCrops(int id)
    {
        return _crops[id];
    }
    #endregion
}
