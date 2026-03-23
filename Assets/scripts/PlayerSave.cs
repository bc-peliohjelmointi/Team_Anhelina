using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSave : MonoBehaviour
{
    public Transform player;
    public float autoSaveTime = 10f;

    float timer;
    bool isNewGame = false;

    void Start()
    {
        // Nếu quên kéo player trong Inspector thì tự lấy
        if (player == null)
            player = transform;

        // 🔥 CHECK NEW GAME
        isNewGame = PlayerPrefs.GetInt("NewGame", 0) == 1;

        if (isNewGame)
        {
            Debug.Log("NEW GAME → KHÔNG LOAD SAVE");

            // reset flag để lần sau continue bình thường
            PlayerPrefs.SetInt("NewGame", 0);
        }
        else
        {
            LoadGame(); // chỉ load khi Continue
        }
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