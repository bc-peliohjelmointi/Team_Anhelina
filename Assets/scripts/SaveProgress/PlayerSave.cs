using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSave : MonoBehaviour
{
    public Transform player;          // Player transform to save
    public Transform startPosition;   // Start position for new game
    public float autoSaveTime = 10f;  // Interval between autosaves
    public bool allowSave = true;     // Enable/disable saving
    float timer;                      // Internal timer for autosave

    void Start()
    {
        if (player == null)
            player = transform;      // default to this object

        bool isNewGame = PlayerPrefs.GetInt("NewGame", 0) == 1;

        if (isNewGame)
        {
            Debug.Log("NEW GAME → No LOAD SAVE");

            allowSave = false;      // skip auto-save until allowed

            if (startPosition != null)
                player.position = startPosition.position; // set start pos

            return; // skip loading old save
        }

        // Continue → load existing save
        LoadGame();
    }

    void Update()
    {
        if (!allowSave) return; // skip if saving disabled

        timer += Time.deltaTime;

        // Auto-save at interval
        if (timer >= autoSaveTime)
        {
            SaveGame();
            timer = 0f;
        }
    }

    // Save player's position and current scene
    void SaveGame()
    {
        PlayerPrefs.SetFloat("PlayerX", player.position.x);
        PlayerPrefs.SetFloat("PlayerY", player.position.y);
        PlayerPrefs.SetFloat("PlayerZ", player.position.z);

        PlayerPrefs.SetString("CurrentScene", SceneManager.GetActiveScene().name);

        PlayerPrefs.Save(); // write to disk

        Debug.Log("Autosaved!");
    }

    // Load saved position if available
    void LoadGame()
    {
        if (!PlayerPrefs.HasKey("PlayerX")) return;

        float x = PlayerPrefs.GetFloat("PlayerX");
        float y = PlayerPrefs.GetFloat("PlayerY");
        float z = PlayerPrefs.GetFloat("PlayerZ");

        player.position = new Vector3(x, y, z);

        Debug.Log("Loaded Save");
    }

    // Save game automatically on quit
    void OnApplicationQuit()
    {
        SaveGame();
    }
}