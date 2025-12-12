using UnityEngine;

public class ShowCanvasOnHit : MonoBehaviour
{
    [Header("References")]
    public GameObject uiCanvas;           // Drag GameOverCanvas сюда в инспекторе
    public string enemyTag = "Enemy";     // Tag для врага

    [Header("Options")]
    public bool useTrigger = true;        // true -> OnTriggerEnter, false -> OnCollisionEnter
    public bool pauseOnShow = true;       // ставить игру на паузу (Time.timeScale = 0)
    public bool disablePlayerControls = true; // попытка отключить скрипты управления

    bool alreadyShown = false;

    void Start()
    {
        if (uiCanvas != null)
            uiCanvas.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!useTrigger || alreadyShown) return;
        if (other.CompareTag(enemyTag))
            ShowCanvas();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (useTrigger || alreadyShown) return;
        if (collision.collider.CompareTag(enemyTag))
            ShowCanvas();
    }

    void ShowCanvas()
    {
        if (uiCanvas == null) return;
        alreadyShown = true;
        uiCanvas.SetActive(true);

        if (pauseOnShow)
            Time.timeScale = 0f;

        if (disablePlayerControls)
        {
            // попытаться найти скрипты управления на этом объекте и отключить их
            var comps = GetComponents<MonoBehaviour>();
            foreach (var c in comps)
            {
                // не отключаем этот скрипт
                if (c != this) c.enabled = false;
            }
        }
    }

    // Вызов для восстановления (если нужно)
    public void HideCanvasAndResume()
    {
        if (uiCanvas != null)
            uiCanvas.SetActive(false);

        Time.timeScale = 1f;
        alreadyShown = false;

        if (disablePlayerControls)
        {
            var comps = GetComponents<MonoBehaviour>();
            foreach (var c in comps)
            {
                if (c != this) c.enabled = true;
            }
        }
    }
}
