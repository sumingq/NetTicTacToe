using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    public GameObject moveHomeImage;
    private Tcp tcp;

    public void OnPlayButton()
    {
        SceneManager.LoadScene(1);
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }

    public void OnRestartButton()
    {
        SceneManager.LoadScene(1);

    }
    public void OnMoveHomeButton()
    {
        if (moveHomeImage != null)
        {
            moveHomeImage.SetActive(true);
        }
    }
    public void OnMoveHomeYesButton()
    {
        SceneManager.LoadScene(0);
    }
    public void OnMoveHomeNoButton()
    {
        moveHomeImage.SetActive(false);
    }
}
