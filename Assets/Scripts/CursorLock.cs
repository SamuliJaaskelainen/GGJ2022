using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorLock : MonoBehaviour
{
    void Update()
    {
        if (UnityEngine.SceneManagement.SceneManager.sceneCount > 1)
        {
            Cursor.lockState = CursorLockMode.None;
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void OnApplicationQuit()
    {
        Cursor.lockState = CursorLockMode.None;
    }
}
