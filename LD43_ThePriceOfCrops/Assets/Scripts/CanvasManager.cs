using UnityEngine;
using UnityEngine.SceneManagement;

public class CanvasManager : MonoBehaviour {

    public static CanvasManager inst;

    public GameObject menu;

    private bool _startedGame;

    private void Awake()
    {
        inst = this;
    }
    private void Start()
    {
        GameManager.timeScale = 0;
    }
    private void Update ()
    {
		if (Input.GetKeyDown(KeyCode.Escape) && _startedGame)
        {
            menu.transform.parent.gameObject.SetActive(!menu.transform.parent.gameObject.activeSelf);
            menu.SetActive(menu.transform.parent.gameObject);
            GameManager.timeScale = menu.activeSelf ? 0 : 1;
        }
	}
    public void StartGame()
    {
        _startedGame = true;
        GameManager.timeScale = 1;
    }
    public void Reload()
    {
        SceneManager.LoadScene(0);
    }
    public void Quit()
    {
        Application.Quit();
    }
}
