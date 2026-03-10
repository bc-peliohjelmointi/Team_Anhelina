using UnityEngine;

public class ShowCanvasOnHit : MonoBehaviour
{
    [Header("References")]
    public GameObject uiCanvas;          
    public string enemyTag = "Enemy";     

    [Header("Options")]
    public bool useTrigger = true;     
    public bool pauseOnShow = true;      
    public bool disablePlayerControls = true;

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
            
            var comps = GetComponents<MonoBehaviour>();
            foreach (var c in comps)
            {
             
                if (c != this) c.enabled = false;
            }
        }
    }

   
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
