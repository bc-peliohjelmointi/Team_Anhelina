using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerLoad : MonoBehaviour
{
    CharacterController controller;

    IEnumerator Start()
    {
        controller = GetComponent<CharacterController>();

        yield return null;

        if (PlayerPrefs.HasKey("PlayerX") && PlayerPrefs.HasKey("CurrentScene"))
        {
            string savedScene = PlayerPrefs.GetString("CurrentScene");
            string currentScene = SceneManager.GetActiveScene().name;

            if (savedScene == currentScene)
            {
                float x = PlayerPrefs.GetFloat("PlayerX");
                float y = PlayerPrefs.GetFloat("PlayerY");
                float z = PlayerPrefs.GetFloat("PlayerZ");

                controller.enabled = false;
                transform.position = new Vector3(x, y, z);
                controller.enabled = true;
            }
        }
    }
}