using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheat : Crops {

    private Vector3 _startVector = new Vector3(1,0,1);

    protected override void Grow()
    {
        _startVector.y = Mathf.Lerp(.1f, 1, Mathf.Min(Time.time / growEnd, 1));
        _transform.localScale = _startVector;
    }
}
