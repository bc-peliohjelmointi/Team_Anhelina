using UnityEngine;
using System.Collections;

public class PuzzleLevel3 : MonoBehaviour
{
    [Header("Small Levers")]
    public Lever smallLever1;
    public Lever smallLever2;
    public Lever smallLever3;
    public Lever smallLever4;

    [Header("Double Levers")]
    public DoubleLever doubleLever1;
    public DoubleLever doubleLever2;

    [Header("Combination: н, в, н, в, н, в")]
    private bool[] correctCombination = new bool[6] { false, true, false, true, false, true };

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
    private GameObject checkOutline;
    private Coroutine validationCoroutine;

    void Start()
    {
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

        float delayBetweenLevers = (validationHoldTime - 0.5f) / 6f;

        if (smallLever1 != null) { smallLever1.SetLightGreen(true); yield return new WaitForSeconds(delayBetweenLevers); }
        if (doubleLever1 != null) { doubleLever1.SetTopLightGreen(true); doubleLever1.SetBottomLightGreen(true); yield return new WaitForSeconds(delayBetweenLevers); }
        if (smallLever2 != null) { smallLever2.SetLightGreen(true); yield return new WaitForSeconds(delayBetweenLevers); }
        if (smallLever3 != null) { smallLever3.SetLightGreen(true); yield return new WaitForSeconds(delayBetweenLevers); }
        if (doubleLever2 != null) { doubleLever2.SetTopLightGreen(true); doubleLever2.SetBottomLightGreen(true); yield return new WaitForSeconds(delayBetweenLevers); }
        if (smallLever4 != null) { smallLever4.SetLightGreen(true); }

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
        if (smallLever1 == null || smallLever1.isUp != correctCombination[0]) return false;
        if (doubleLever1 == null || doubleLever1.isUp != correctCombination[1]) return false;
        if (smallLever2 == null || smallLever2.isUp != correctCombination[2]) return false;
        if (smallLever3 == null || smallLever3.isUp != correctCombination[3]) return false;
        if (doubleLever2 == null || doubleLever2.isUp != correctCombination[4]) return false;
        if (smallLever4 == null || smallLever4.isUp != correctCombination[5]) return false;

        return true;
    }

    void UpdateAllLights()
    {
        if (isValidating) return;

        bool prev = true;

        if (smallLever1 != null)
        {
            bool correct = smallLever1.isUp == correctCombination[0];
            smallLever1.SetLightGreen(correct && prev);
            if (!correct) prev = false;
        }

        if (doubleLever1 != null)
        {
            bool correct = doubleLever1.isUp == correctCombination[1];
            doubleLever1.SetTopLightGreen(correct && prev);
            doubleLever1.SetBottomLightGreen(correct && prev);
            if (!correct) prev = false;
        }

        if (smallLever2 != null)
        {
            bool correct = smallLever2.isUp == correctCombination[2];
            smallLever2.SetLightGreen(correct && prev);
            if (!correct) prev = false;
        }

        if (smallLever3 != null)
        {
            bool correct = smallLever3.isUp == correctCombination[3];
            smallLever3.SetLightGreen(correct && prev);
            if (!correct) prev = false;
        }

        if (doubleLever2 != null)
        {
            bool correct = doubleLever2.isUp == correctCombination[4];
            doubleLever2.SetTopLightGreen(correct && prev);
            doubleLever2.SetBottomLightGreen(correct && prev);
            if (!correct) prev = false;
        }

        if (smallLever4 != null)
        {
            bool correct = smallLever4.isUp == correctCombination[5];
            smallLever4.SetLightGreen(correct && prev);
        }
    }

    void SetAllLightsGreen()
    {
        if (smallLever1 != null) smallLever1.SetLightGreen(true);
        if (doubleLever1 != null) { doubleLever1.SetTopLightGreen(true); doubleLever1.SetBottomLightGreen(true); }
        if (smallLever2 != null) smallLever2.SetLightGreen(true);
        if (smallLever3 != null) smallLever3.SetLightGreen(true);
        if (doubleLever2 != null) { doubleLever2.SetTopLightGreen(true); doubleLever2.SetBottomLightGreen(true); }
        if (smallLever4 != null) smallLever4.SetLightGreen(true);
    }

    void SetAllLightsRed()
    {
        if (smallLever1 != null) smallLever1.SetLightGreen(false);
        if (doubleLever1 != null) { doubleLever1.SetTopLightGreen(false); doubleLever1.SetBottomLightGreen(false); }
        if (smallLever2 != null) smallLever2.SetLightGreen(false);
        if (smallLever3 != null) smallLever3.SetLightGreen(false);
        if (doubleLever2 != null) { doubleLever2.SetTopLightGreen(false); doubleLever2.SetBottomLightGreen(false); }
        if (smallLever4 != null) smallLever4.SetLightGreen(false);
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