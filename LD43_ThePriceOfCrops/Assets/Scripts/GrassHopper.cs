using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassHopper : MonoBehaviour
{
    #region Vars
    private Field _targetField;
    private Transform _transform;
    private Animator _anim;
    #endregion
    #region MonoFunctions
    private void Start()
    {
        _transform = GetComponent<Transform>();
        _anim = GetComponent<Animator>();
        gameObject.SetActive(false);
    }
    #endregion
    #region Functions
    public void SetTargetField(Field field)
    {
        _targetField = field;

        field.Attaked();
        SetPosition(field.transform.position);
        gameObject.SetActive(true);
        _anim.SetTrigger("Attack");
    }
    private void SetPosition(Vector3 pos)
    {
        _transform.position = pos;
    }
    public void ToggleEnable()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
    public void KillCrops()
    {
        _targetField.CropsDamaged();
    }
    public void EndAttack()
    {
        _targetField.EndAttaked();
    }
    #endregion
}
