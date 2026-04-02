using UnityEngine;
using System.Collections;
public class PuzzleLevel1 : MonoBehaviour
{
    [Header("8 Small Levers")]
    public Lever lever1, lever2, lever3, lever4, lever5, lever6, lever7, lever8;
    [Header("Combination (false=down, true=up)")]
    public bool[] correctCombination = new bool[8] { false, true, true, true, false, true, false, true };

    [Header("Check Lever")]
    public Transform checkLeverTransform;
    public MeshFilter checkLeverMeshFilter;
    public Vector3 checkDownRotation = new Vector3(45f, 0f, 0f);
    public Vector3 checkUpRotation = new Vector3(-45f, 0f, 0f);
    public float checkLeverSpeed = 5f;

    [Header("Validation")]
    public float validationHoldTime = 5f;

    [Header("Check Lever Highlight")]
    public Color checkHighlightColor = Color.yellow;
    public float checkOutlineWidth = 0.02f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public float soundVolume = 0.5f;

    [Header("Panel")]
    public MainControlPanel controlPanel;

    private bool isSolved = false;
    private bool isValidating = false;
    private Quaternion checkTargetRotation;
    private Lever[] allLevers;
    private GameObject checkOutlineObj;
    private Coroutine validationCoroutine;

    void Start()
    {
        allLevers = new Lever[8] { lever1, lever2, lever3, lever4, lever5, lever6, lever7, lever8 };
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        checkTargetRotation = Quaternion.Euler(checkDownRotation);
        if (checkLeverTransform != null) checkLeverTransform.localRotation = checkTargetRotation;
        UpdateAllLights();
    }

    void Update()
    {
        if (checkLeverTransform != null)
            checkLeverTransform.localRotation = Quaternion.Lerp(
                checkLeverTransform.localRotation, checkTargetRotation, Time.deltaTime * checkLeverSpeed);
    }

    public void PullCheckLever()
    {
        if (isSolved || isValidating) return;
        checkTargetRotation = Quaternion.Euler(checkUpRotation);
        if (CheckIfCorrect())
        {
            if (validationCoroutine != null) StopCoroutine(validationCoroutine);
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
            if (wrongSound != null) audioSource.PlayOneShot(wrongSound, soundVolume);
            UpdateAllLights();
        }
    }

    IEnumerator ValidationSequence()
    {
        isValidating = true;
        SetAllLights(false);
        yield return new WaitForSeconds(0.5f);
        float delay = (validationHoldTime - 0.5f) / 8f;
        for (int i = 0; i < 8; i++)
        {
            if (allLevers[i] != null) allLevers[i].SetLightGreen(true);
            yield return new WaitForSeconds(delay);
        }
        isSolved = true;
        isValidating = false;
        if (correctSound != null) audioSource.PlayOneShot(correctSound, soundVolume);
        if (controlPanel != null) controlPanel.NotifyLevelSolved();
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

    public void OnLeverChanged()
    {
        if (isSolved || isValidating) return;
        UpdateAllLights();
    }

    void UpdateAllLights()
    {
        bool prev = true;
        for (int i = 0; i < 8; i++)
        {
            if (allLevers[i] == null) continue;
            bool correct = allLevers[i].isUp == correctCombination[i];
            allLevers[i].SetLightGreen(correct && prev);
            if (!correct) prev = false;
        }
    }

    void SetAllLights(bool green)
    {
        foreach (Lever l in allLevers)
            if (l != null) l.SetLightGreen(green);
    }

    public bool IsSolved() => isSolved;

    public void HighlightCheckLever(bool enable)
    {
        if (enable) CreateCheckOutline();
        else RemoveCheckOutline();
    }

    void CreateCheckOutline()
    {
        if (checkOutlineObj != null || checkLeverMeshFilter == null || checkLeverTransform == null) return;
        checkOutlineObj = new GameObject("CheckLeverOutline");
        checkOutlineObj.transform.SetParent(checkLeverTransform);
        checkOutlineObj.transform.localPosition = Vector3.zero;
        checkOutlineObj.transform.localRotation = Quaternion.identity;
        checkOutlineObj.transform.localScale = Vector3.one * (1f + checkOutlineWidth);
        MeshFilter mf = checkOutlineObj.AddComponent<MeshFilter>();
        mf.mesh = checkLeverMeshFilter.mesh;
        MeshRenderer mr = checkOutlineObj.AddComponent<MeshRenderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = checkHighlightColor;
        mat.SetFloat("_Metallic", 0f);
        mat.SetFloat("_Glossiness", 0.8f);
        mr.material = mat;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        checkOutlineObj.layer = checkLeverTransform.gameObject.layer;
    }

    void RemoveCheckOutline()
    {
        if (checkOutlineObj != null) { Destroy(checkOutlineObj); checkOutlineObj = null; }
    }

    void OnDestroy() { RemoveCheckOutline(); }
}