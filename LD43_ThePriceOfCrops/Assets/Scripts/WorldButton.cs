using UnityEngine;
using UnityEngine.Events;

public class WorldButton : MonoBehaviour
{
    #region Vars
    [SerializeField] private UnityEvent onClick;
    public SpriteRenderer sprite;
    public Color desable, enable;
    public bool _enabled;
    #endregion
    #region Functions
    public void OnClick()
    {
        onClick.Invoke();
    }
    public void Toogle()
    {
        _enabled = !_enabled;
        sprite.color = _enabled ? enable : desable;
    }
    #endregion
}
