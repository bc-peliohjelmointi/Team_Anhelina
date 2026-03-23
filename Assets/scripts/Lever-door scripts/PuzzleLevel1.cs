using UnityEngine;
using System.Collections;

public class PuzzleLevel1 : MonoBehaviour
{
    [Header("8 Small Levers")]
    public Lever lever1;
    public Lever lever2;
    public Lever lever3;
    public Lever lever4;
    public Lever lever5;
    public Lever lever6;
    public Lever lever7;
    public Lever lever8;

    [Header("Combination: н, в, в, в, н, в, н, в")]
    private bool[] correctCombination = new bool[8] { false, true, true, true, false, true, false, true };

    [Header("Check Lever")]
    public Transform checkLever;
    public MeshFilter checkLeverMeshFilter;
    public Vector3 checkDownRotation = new Vector3(45, 0, 0);
    public Vector3 checkUpRotation = new Vector3(-45, 0, 0);
    public float checkLeverSpeed = 5f;

    [Header("Validation Settings")]
    public float validationHoldTime = 5f;

    [Header("Check Lever Highlight")]
    public Color checkHighlightColor = Color.yellow;
    public float checkOutlineWidth = 0.02f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public float soundVolume = 0.5f;

    private bool isSolved = false;
    private bool isValidating = false;
    private Quaternion checkTargetRotation;
    private Lever[] allLevers;
    private GameObject checkOutline;
    private Coroutine validationCoroutine;

    void Start()
    {
        allLevers = new Lever[8] { lever1, lever2, lever3, lever4, lever5, lever6, lever7, lever8 };

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.volume = soundVolume;

        checkTargetRotation = Quaternion.Euler(checkDownRotation);
        if (checkLever != null)
        {
            checkLever.localRotation = checkTargetRotation;
        }

        UpdateAllLights();
    }

    void Update()
    {
        if (checkLever != null)
        {
            checkLever.localRotation = Quaternion.Lerp(
                checkLever.localRotation,
                checkTargetRotation,
                Time.deltaTime * checkLeverSpeed
            );
        }
    }

    public void PullCheckLever()
    {
        if (isSolved || isValidating) return;

        checkTargetRotation = Quaternion.Euler(checkUpRotation);

        if (CheckIfCorrect())
        {
            if (validationCoroutine != null)
            {
                StopCoroutine(validationCoroutine);
            }
            validationCoroutine = StartCoroutine(ValidationSequence());
        }
    }

    public void ReleaseCheckLever()
    {
        if (isSolved) return;

        checkTargetRotation = Quaternion.Euler(checkDownRotation);

        if (validationCoroutine != null)
        {
            StopCoroutine(validationCoroutine);
            validationCoroutine = null;
            isValidating = false;

            if (wrongSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(wrongSound, soundVolume);
            }

            UpdateAllLights();
        }
    }

    IEnumerator ValidationSequence()
    {
        isValidating = true;

        SetAllLightsRed();

        yield return new WaitForSeconds(0.5f);

        float delayBetweenLevers = (validationHoldTime - 0.5f) / 8f;

        for (int i = 0; i < 8; i++)
        {
            if (allLevers[i] != null)
            {
                allLevers[i].SetLightGreen(true);
            }
            yield return new WaitForSeconds(delayBetweenLevers);
        }

        isSolved = true;
        isValidating = false;

        if (correctSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(correctSound, soundVolume);
        }

        validationCoroutine = null;
    }

    bool CheckIfCorrect()
    {
        for (int i = 0; i < 8; i++)
        {
            if (allLevers[i] == null) return false;
            if (allLevers[i].isUp != correctCombination[i]) return false;
        }
        return true;
    }

    void UpdateAllLights()
    {
        if (isValidating) return;

        bool previousCorrect = true;

        for (int i = 0; i < 8; i++)
        {
            if (allLevers[i] == null) continue;

            bool currentCorrect = allLevers[i].isUp == correctCombination[i];
            bool shouldBeGreen = currentCorrect && previousCorrect;

            allLevers[i].SetLightGreen(shouldBeGreen);

            if (!currentCorrect)
            {
                previousCorrect = false;
            }
        }
    }

    void SetAllLightsGreen()
    {
        for (int i = 0; i < 8; i++)
        {
            if (allLevers[i] != null)
            {
                allLevers[i].SetLightGreen(true);
            }
        }
    }

    void SetAllLightsRed()
    {
        for (int i = 0; i < 8; i++)
        {
            if (allLevers[i] != null)
            {
                allLevers[i].SetLightGreen(false);
            }
        }
    }

    public bool IsSolved()
    {
        return isSolved;
    }

    public void OnLeverChanged()
    {
        if (!isSolved)
        {
            UpdateAllLights();
        }
    }

    public void HighlightCheckLever(bool enable)
    {
        if (enable)
        {
            CreateCheckOutline();
        }
        else
        {
            RemoveCheckOutline();
        }
    }

    void CreateCheckOutline()
    {
        if (checkOutline != null || checkLeverMeshFilter == null || checkLever == null) return;

        checkOutline = new GameObject("CheckLeverOutline");
        checkOutline.transform.SetParent(checkLever);
        checkOutline.transform.localPosition = Vector3.zero;
        checkOutline.transform.localRotation = Quaternion.identity;
        checkOutline.transform.localScale = Vector3.one * (1f + checkOutlineWidth);

        MeshFilter outlineMeshFilter = checkOutline.AddComponent<MeshFilter>();
        outlineMeshFilter.mesh = checkLeverMeshFilter.mesh;

        MeshRenderer outlineRenderer = checkOutline.AddComponent<MeshRenderer>();
        Material outlineMaterial = new Material(Shader.Find("Standard"));
        outlineMaterial.color = checkHighlightColor;
        outlineMaterial.SetFloat("_Metallic", 0f);
        outlineMaterial.SetFloat("_Glossiness", 0.8f);
        outlineRenderer.material = outlineMaterial;
        outlineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        checkOutline.layer = checkLever.gameObject.layer;
    }

    void RemoveCheckOutline()
    {
        if (checkOutline != null)
        {
            Destroy(checkOutline);
            checkOutline = null;
        }
    }

    void OnDestroy()
    {
        RemoveCheckOutline();
    }
}