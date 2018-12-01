using UnityEngine;
using UnityEngine.Events;

public class WorldButton : MonoBehaviour
{
    #region Vars
    [SerializeField] private UnityEvent onClick;
    #endregion
    #region Functions
    public void OnClick()
    {
        onClick.Invoke();
    }
    #endregion
}
