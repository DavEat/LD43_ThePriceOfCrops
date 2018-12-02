using UnityEngine;
using System.Collections;

public class Sacrifice : MonoBehaviour
{
    public static Sacrifice inst;

    #region Vars
    public int startGodsHappiness = 550;
    public int crtGodsHappiness;
    public int happinessDecreaseBySec = 1;

    public int startUnhappyAt = 500;
    public int timeBetweenDisaster = 20;
    public int nextDisasterMinIn = 0;

    public int happinessByFarmerMul = 60, happinessFoodFarmerDiv = 5;

    [Space(10)]
    [Header("Disease")]
    public GrassHopper grassHopper;

    [SerializeField] private Transform[] _particules;

    [Space(10)]
    [SerializeField] private GameObject _interface;

    #if UNITY_EDITOR
    [Space(10)]
    public bool unhappy = false;
    #endif
    #endregion
    #region MonoFunctions
    private void Awake()
    {
        inst = this;
    }
    private void Start()
    {
        crtGodsHappiness = startGodsHappiness;
        nextDisasterMinIn = timeBetweenDisaster;

        StartCoroutine(DescreseHappiness());
    }
    #endregion
    #region Functions
    private IEnumerator DescreseHappiness()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            crtGodsHappiness -= happinessDecreaseBySec;

            float percent = 1 - ((float)crtGodsHappiness / 501); //percent of god a desaster

            
            float scaleZ = .4f;
            if (percent > 0)
                scaleZ = 1 + percent * 2;
            Vector3 scale = new Vector3(1, 1, scaleZ);
            _particules[0].localScale = scale;
            _particules[1].localScale = scale;

            if (nextDisasterMinIn > 0)
                nextDisasterMinIn -= happinessDecreaseBySec;
            else if (crtGodsHappiness <= startUnhappyAt)
            {
                IsUnhappy(percent);
            }

            #if UNITY_EDITOR
            if (unhappy)
            {
                unhappy = false;
                IsUnhappy(2);
            }
            #endif
        }
    }
    internal void IsUnhappy(float percent)
    {
        if (Random.Range(0f, 1) < percent)
        {
            Debug.Log("Disaster");
            nextDisasterMinIn = timeBetweenDisaster;

            DiseaseAppend();
        }
    }
    internal void SacrificeFarmer(Farmer farmer)
    {
        //Add anim
        farmer.Kill();
        crtGodsHappiness += happinessDecreaseBySec * happinessByFarmerMul;
    }
    internal void SacrificeFood(Crops crops)
    {
        //Add anim
        crtGodsHappiness += happinessDecreaseBySec * (crops.GetFoodValue() / happinessFoodFarmerDiv);
    }
    internal void DiseaseAppend()
    {
        Field field = FoodManager.inst.GetMostFullField();
        if (field != null)
        {
            grassHopper.SetTargetField(field);
        }
        else
        {
            Debug.Log("Kill farmer");
            GameManager.inst.KillFirstFarmer();
        }
    }
    internal void ShowInterface(bool value)
    {
        if (_interface.activeSelf != value)
            _interface.SetActive(value);
    }
    internal bool InterfaceDisplayed()
    {
        return _interface.activeSelf;
    }
    #endregion
}
