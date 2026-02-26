using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject infoTextObject; // текст слева
    private Text uiText;

    void Start()
    {
        if (infoTextObject != null)
        {
            infoTextObject.SetActive(false);
            uiText = infoTextObject.GetComponent<Text>();
        }
    }

    public void PlayGame()
    {
        LevelManager.Instance.LoadScene("sCENE1", "CrossFade");
        SceneManager.LoadScene("sCENE1");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowInfo(string text)
    {
        if (infoTextObject != null)
        {
            infoTextObject.SetActive(true);
            uiText.text = text;
        }
    }

    public void HideInfo()
    {
        if (infoTextObject != null)
        {
            infoTextObject.SetActive(false);
        }
    }
}