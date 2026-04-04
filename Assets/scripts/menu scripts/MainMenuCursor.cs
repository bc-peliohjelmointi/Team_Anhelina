using UnityEngine;

public class MainMenuCursor : MonoBehaviour
{
    void OnEnable()
    {
        // unlock & show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // reset time in case of pause
        Time.timeScale = 1f;
    }

    void UnlockCursor()
    {
        // helper to unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}