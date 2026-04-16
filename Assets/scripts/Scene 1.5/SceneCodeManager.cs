using UnityEngine;
// generates a fresh random 8 digit code each time this scene loads
// no DontDestroyOnLoad on purpose - each scene should have its own code
// ComputerInteraction and CodeDisplay both grab the code from Instance.GetCode()
public class SceneCodeManager : MonoBehaviour
{
    public static SceneCodeManager Instance { get; private set; }
    // number of digits in the generated code
    public int codeLength = 8;

    private string generatedCode = "";

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            // destroy duplicate if somehow two got created
            Destroy(gameObject);
            return;
        }
        // generate immediately in Awake so its ready before any Start() calls it
        GenerateCode();
    }

    void GenerateCode()
    {
        generatedCode = "";
        for (int i = 0; i < codeLength; i++)
            generatedCode += Random.Range(0, 10).ToString();
    }

    public string GetCode() => generatedCode;
}