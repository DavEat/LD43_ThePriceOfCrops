using UnityEngine;

public class FixAngle : MonoBehaviour {

    [SerializeField] private Vector3 _angle;

    private Transform _transform;

    private void Start()
    {
        _transform = GetComponent<Transform>();
    }
	private void Update ()
    {
        if (_transform.eulerAngles != _angle)
            _transform.eulerAngles = _angle;
    }
}
