using UnityEngine;

public class FoodManager : MonoBehaviour {

    public static FoodManager inst;

    public enum CropsType { pumkin, wheat }

    #region Vars
    [SerializeField] private CropsData[] _crops;

    [Header("Field")]
    [SerializeField] private Field[] _fields;
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
    public CropsData[] GetCropsData()
    {
        return _crops;
    }
    public int[] FindFood()
    {
        int id = Grenary.inst.FindBestFood();
        if (id > 0)
        {
            return new int[] { -1, id }; //-1 equal grenary
        }
        else //go to field
        {
            int fieldId = -1;
            int pointId = -1;
            int food = -1;
            for (int i = 0; i < _fields.Length; i++)
            {
                int[] pointIdTemp = _fields[i].FindBestCorp();
                if (pointIdTemp[1] > food)
                {
                    fieldId = i;
                    pointId = pointIdTemp[0];
                }
            }

            if (fieldId >= 0 && pointId >= 0)
                return new int[] { fieldId, pointId };
        }
        return new int[] { -1, -1 };
    }
    public Field GetMostFullField()
    {
        int count = 0;
        int fieldId = -1;
        for (int i = 0; i < _fields.Length; i++)
        {
            int c = _fields[i].CountCorps();
            if (c > count)
            {
                count = c;
                fieldId = i;
            }
        }
        if (fieldId != -1)
            return _fields[fieldId];
        return null;
    }
    public Field.PlantPoint GetFieldPlantPoint(int[] ids)
    {
        return _fields[ids[0]].GetPlantPointById(ids[1]);
    }
    #endregion
}
