using UnityEngine;

[CreateAssetMenu(fileName = "Crops", menuName = "Crops", order = 1)]
public class CropsData : ScriptableObject {

    [Header("Time")]
    public float plantTime = .5f;
    public float growTime = 15;

    [Space(10)]
    [Header("Quantity")]
    public int food = 1;

    public Crops obj;
}
