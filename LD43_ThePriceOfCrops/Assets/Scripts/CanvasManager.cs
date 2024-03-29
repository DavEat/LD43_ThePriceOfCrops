﻿using UnityEngine;
using UnityEngine.SceneManagement;

public class CanvasManager : MonoBehaviour {

    public static CanvasManager inst;

    public GameObject menu, menuButton;

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
        if (Input.GetKeyDown(KeyCode.Escape) && menuButton.activeSelf)
            Escape();
	}
    public void StartGame()
    {
        _startedGame = true;
        GameManager.timeScale = 1;
        GameManager.inst.PauseFarmer(false);
        GameManager.inst.PauseParticles(false);
    }
    public void Reload()
    {
        SceneManager.LoadScene(0);
    }
    public void Quit()
    {
        Application.Quit();
    }
    public void Escape()
    {
        if (_startedGame)
        {
            GameManager.inst.PauseFarmer(true);
            GameManager.inst.PauseParticles(true);
            menu.transform.parent.gameObject.SetActive(!menu.transform.parent.gameObject.activeSelf);
            menu.SetActive(menu.transform.parent.gameObject);
            GameManager.timeScale = menu.activeSelf ? 0 : 1;
            menuButton.SetActive(!menu.activeSelf);
        }
    }
}
