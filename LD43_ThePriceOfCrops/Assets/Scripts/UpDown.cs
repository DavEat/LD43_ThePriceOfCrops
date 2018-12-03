using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpDown : MonoBehaviour {

    public float intencity = 1;
    private Transform _transform;
    private float _startYPos;

	private void Start ()
    {
        _transform = GetComponent<Transform>();
        _startYPos = 0;//_transform.localPosition.y;
    }
	
	private void Update ()
    {
        float y = _startYPos + Mathf.Sin(GameManager.time) * GameManager.deltaTime * intencity;
        _transform.localPosition = new Vector3(_transform.localPosition.x, y, _transform.localPosition.z);
    }
}
