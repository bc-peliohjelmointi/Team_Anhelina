using UnityEngine;

public class MainMenuCursor : MonoBehaviour
{
    void OnEnable()
    {
        // Разблокируем курсор и делаем его видимым
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Сбрасываем время на случай если была пауза
        Time.timeScale = 1f;
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
