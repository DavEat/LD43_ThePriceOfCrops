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
    public int happinessBetweenDisaster = 20;
    public int nextDisasterMinIn = 0;

    public int happinessByFarmerMul = 60;
    #endregion
    #region MonoFunctions
    private void Awake()
    {
        inst = this;
    }
    private void Start()
    {
        crtGodsHappiness = startGodsHappiness;
        nextDisasterMinIn = startGodsHappiness;

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
            if (nextDisasterMinIn > 0)
                nextDisasterMinIn -= happinessDecreaseBySec;
            else if (crtGodsHappiness <= startUnhappyAt)
            {
                IsUnhappy();
            }
        }
    }
    public void IsUnhappy()
    {
        float percent = 1 -((float)crtGodsHappiness / 501); //percent of god a desaster
        if (Random.Range(0f, 1) > percent)
        {
            Debug.Log("Disaster");
            nextDisasterMinIn = happinessBetweenDisaster;
        }
    }

    public void SacrificeFarmer(Farmer farmer)
    {
        //Add anim
        Destroy(farmer.gameObject);
        crtGodsHappiness += happinessDecreaseBySec * happinessByFarmerMul;
    }
    #endregion
}
