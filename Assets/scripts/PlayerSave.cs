using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSave : MonoBehaviour
{
    public Transform player;
    public float autoSaveTime = 10f;

    float timer;

    void Start()
    {
        // Nếu quên kéo player trong Inspector thì tự lấy
        if (player == null)
            player = transform;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= autoSaveTime)
        {
            SaveGame();
            timer = 0f;
        }
    }

    void SaveGame()
    {
        PlayerPrefs.SetFloat("PlayerX", player.position.x);
        PlayerPrefs.SetFloat("PlayerY", player.position.y);
        PlayerPrefs.SetFloat("PlayerZ", player.position.z);

        PlayerPrefs.SetString("CurrentScene", SceneManager.GetActiveScene().name);

        PlayerPrefs.Save();

        Debug.Log("Autosaved!");
    }

    void OnApplicationQuit()
    {
        SaveGame(); // autosave khi thoát game
    }
}