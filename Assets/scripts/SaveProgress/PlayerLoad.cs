using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerLoad : MonoBehaviour   // loads the player’s saved position if continuing a game
{
    CharacterController controller; // reference to character controller

    IEnumerator Start()
    {
        controller = GetComponent<CharacterController>(); // get controller
        yield return null; // wait one frame

        // New Game → skip loading saved position
        if (PlayerPrefs.GetInt("NewGame", 0) == 1)
            yield break;

        // Continue → load saved position if exists
        if (PlayerPrefs.HasKey("PlayerX") && PlayerPrefs.HasKey("CurrentScene"))
        {
            string savedScene = PlayerPrefs.GetString("CurrentScene");
            string currentScene = SceneManager.GetActiveScene().name;

            // only load if player is in saved scene
            if (savedScene == currentScene)
            {
                float x = PlayerPrefs.GetFloat("PlayerX"); // saved X
                float y = PlayerPrefs.GetFloat("PlayerY"); // saved Y
                float z = PlayerPrefs.GetFloat("PlayerZ"); // saved Z

                controller.enabled = false; // disable controller to teleport
                transform.position = new Vector3(x, y, z); // set position
                controller.enabled = true; // re-enable controller
            }
        }
    }
}