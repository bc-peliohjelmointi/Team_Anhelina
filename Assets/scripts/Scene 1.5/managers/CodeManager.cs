using UnityEngine;
public class CodeManager : MonoBehaviour
{
    public static CodeManager Instance { get; private set; }
    private const string PREFS_KEY = "RoomAccessCode";
    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    public string GenerateAndSave()
    {
        string code = Random.Range(10000000, 99999999).ToString();
        PlayerPrefs.SetString(PREFS_KEY, code);
        PlayerPrefs.Save();
        return code;
    }

    public string GetCode() => PlayerPrefs.GetString(PREFS_KEY, "00000000");
    public string GetSavedCode() => PlayerPrefs.GetString(PREFS_KEY, "????????");
}