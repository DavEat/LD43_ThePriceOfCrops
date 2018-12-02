using UnityEngine;

public class Rotation : MonoBehaviour {

    public float multiplicator = 1;
    public Vector3 rotation;

    private Transform _transform;

    private void Start()
    {
        _transform = GetComponent<Transform>();
    }

	private void Update ()
    {
        _transform.Rotate(rotation * multiplicator);
    }
}
