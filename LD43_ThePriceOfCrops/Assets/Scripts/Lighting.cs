using System.Collections;
using UnityEngine;

public class Lighting : MonoBehaviour {

    public static Lighting inst;

    private MeshRenderer _meshRenderer;

    private void Awake()
    {
        inst = this;
    }
    private void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.enabled = false;
    }
    public void Hit(Vector3 position)
    {
        transform.position = position + Vector3.up * 2;
        _meshRenderer.enabled = true;
        StartCoroutine(Desable());
    }

    private IEnumerator Desable()
    {
        yield return new WaitForSeconds(1);
        _meshRenderer.enabled = false;
    }
}
