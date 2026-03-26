using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSave : MonoBehaviour
{
    public Transform player;
    public Transform startPosition;
    public float autoSaveTime = 10f;
    public bool allowSave = true;
    float timer;

    void Start()
    {
        if (player == null)
            player = transform;

        bool isNewGame = PlayerPrefs.GetInt("NewGame", 0) == 1;

        if (isNewGame)
        {
            Debug.Log("NEW GAME → No LOAD SAVE");

            allowSave = false;

            if (startPosition != null)
                player.position = startPosition.position;

            return; // skip load save
        }
        // continue
        LoadGame();
    }

    void Update()
    {

        if (!allowSave) return;

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

    void LoadGame()
    {
        if (!PlayerPrefs.HasKey("PlayerX")) return;

        float x = PlayerPrefs.GetFloat("PlayerX");
        float y = PlayerPrefs.GetFloat("PlayerY");
        float z = PlayerPrefs.GetFloat("PlayerZ");

        player.position = new Vector3(x, y, z);

        Debug.Log("Loaded Save");
    }

    void OnApplicationQuit()
    {
        SaveGame();
    }
}