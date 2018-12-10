using UnityEngine;
using UnityEngine.UI;

public class StateIcon : MonoBehaviour {

    public enum States { none, eat, plant, storing, trash, reproduction, sacrifice, death }

    [SerializeField] private Icon[] _icons;

    private Image _image;
    private GameObject _parent;

    private void Start ()
    {
        _image = GetComponent<Image>();
        _parent = transform.parent.gameObject;
        _parent.SetActive(false);
    }
    public void UpdateState(States state)
    {
        if (state == States.none)
            _parent.SetActive(false);
        else
        {
            if (!_parent.activeSelf)
                _parent.SetActive(true);

            int index = (int)state - 1;
            _image.color = _icons[index].color;
            _image.sprite = _icons[index].sprite;
        }
    }

    [System.Serializable]
    private struct Icon
    {
        public Color color;
        public Sprite sprite;

        public Icon(Color color, Sprite sprite)
        {
            this.color = color;
            this.sprite = sprite;
        }
    }
}

 
