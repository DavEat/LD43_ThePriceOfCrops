using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanAnim : MonoBehaviour {

    private Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public void TriggerCarry()
    {
        _animator.SetTrigger("Carry");
    }
}
