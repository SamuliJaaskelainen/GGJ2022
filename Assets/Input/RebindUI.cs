using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RebindUI : MonoBehaviour
{
    public void LoadMainScene()
    {
        Debug.Log("Load main");
        Time.timeScale = 1.0f;
        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("Rebind");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LoadMainScene();
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
