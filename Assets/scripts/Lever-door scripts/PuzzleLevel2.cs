using UnityEngine;
using System.Collections;

public class PuzzleLevel2 : MonoBehaviour
{
    [Header("4 Double Levers")]
    public DoubleLever lever1;
    public DoubleLever lever2;
    public DoubleLever lever3;
    public DoubleLever lever4;

    [Header("Combination: в, в, н, в")]
    private bool[] correctCombination = new bool[4] { true, true, false, true };

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
    private DoubleLever[] allLevers;
    private GameObject checkOutline;
    private Coroutine validationCoroutine;

    void Start()
    {
        allLevers = new DoubleLever[4] { lever1, lever2, lever3, lever4 };

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

        float delayBetweenLevers = (validationHoldTime - 0.5f) / 4f;

        for (int i = 0; i < 4; i++)
        {
            if (allLevers[i] != null)
            {
                allLevers[i].SetTopLightGreen(true);
                allLevers[i].SetBottomLightGreen(true);
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
        for (int i = 0; i < 4; i++)
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

        for (int i = 0; i < 4; i++)
        {
            if (allLevers[i] == null) continue;

            bool currentCorrect = allLevers[i].isUp == correctCombination[i];
            bool shouldBeGreen = currentCorrect && previousCorrect;

            allLevers[i].SetTopLightGreen(shouldBeGreen);
            allLevers[i].SetBottomLightGreen(shouldBeGreen);

            if (!currentCorrect)
            {
                previousCorrect = false;
            }
        }
    }

    void SetAllLightsGreen()
    {
        for (int i = 0; i < 4; i++)
        {
            if (allLevers[i] != null)
            {
                allLevers[i].SetTopLightGreen(true);
                allLevers[i].SetBottomLightGreen(true);
            }
        }
    }

    void SetAllLightsRed()
    {
        for (int i = 0; i < 4; i++)
        {
            if (allLevers[i] != null)
            {
                allLevers[i].SetTopLightGreen(false);
                allLevers[i].SetBottomLightGreen(false);
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