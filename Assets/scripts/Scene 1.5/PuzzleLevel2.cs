using UnityEngine;
using System.Collections;
// controls row 2 of the puzzle - 4 double levers
// same logic as row 1 but fewer levers and each has two lights
// both top and bottom lights go green together on a correct lever
public class PuzzleLevel2 : MonoBehaviour
{
    // 4 double levers in row 2, drag left to right
    public DoubleLever lever1, lever2, lever3, lever4;
    // the correct up/down combination for row 2
    public bool[] correctCombination = new bool[4] { true, true, false, true };
    public Transform checkLeverTransform;
    public MeshFilter checkLeverMeshFilter;
    public Vector3 checkDownRotation = new Vector3(45f, 0f, 0f);
    public Vector3 checkUpRotation = new Vector3(-45f, 0f, 0f);
    public float checkLeverSpeed = 5f;
    public float validationHoldTime = 5f;
    public Color checkHighlightColor = Color.yellow;
    public float checkOutlineWidth = 0.02f;

    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public float soundVolume = 0.5f;
    public MainControlPanel controlPanel;

    private bool isSolved = false;
    private bool isValidating = false;
    private Quaternion checkTargetRotation;
    private DoubleLever[] allLevers;
    private GameObject checkOutlineObj;
    private Coroutine validationCoroutine;

    void Start()
    {
        allLevers = new DoubleLever[4] { lever1, lever2, lever3, lever4 };
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
        if (!CheckIfCorrect()) return;
        if (validationCoroutine != null) StopCoroutine(validationCoroutine);
        validationCoroutine = StartCoroutine(ValidationSequence());
    }

    public void ReleaseCheckLever()
    {
        if (isSolved) return;
        checkTargetRotation = Quaternion.Euler(checkDownRotation);
        if (validationCoroutine == null) return;
        StopCoroutine(validationCoroutine);
        validationCoroutine = null;
        isValidating = false;
        if (wrongSound != null) audioSource.PlayOneShot(wrongSound, soundVolume);
        UpdateAllLights();
    }

    IEnumerator ValidationSequence()
    {
        isValidating = true;
        SetAllLights(false);
        yield return new WaitForSeconds(0.5f);
        float delay = (validationHoldTime - 0.5f) / 4f;
        for (int i = 0; i < 4; i++)
        {
            if (allLevers[i] != null)
            {
                // light both top and bottom green at the same time
                allLevers[i].SetTopLightGreen(true);
                allLevers[i].SetBottomLightGreen(true);
            }
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
        for (int i = 0; i < 4; i++)
        {
            if (allLevers[i] == null) return false;
            if (allLevers[i].isUp != correctCombination[i]) return false;
        }
        return true;
    }

    public void OnLeverChanged()
    {
        if (!isSolved && !isValidating) UpdateAllLights();
    }

    void UpdateAllLights()
    {
        bool prev = true;
        for (int i = 0; i < 4; i++)
        {
            if (allLevers[i] == null) continue;
            bool correct = allLevers[i].isUp == correctCombination[i];
            allLevers[i].SetTopLightGreen(correct && prev);
            allLevers[i].SetBottomLightGreen(correct && prev);
            if (!correct) prev = false;
        }
    }

    void SetAllLights(bool green)
    {
        foreach (DoubleLever l in allLevers)
        {
            if (l == null) continue;
            l.SetTopLightGreen(green);
            l.SetBottomLightGreen(green);
        }
    }

    public bool IsSolved() => isSolved;

    public void HighlightCheckLever(bool enable)
    {
        if (enable) CreateCheckOutline(); else RemoveCheckOutline();
    }

    void CreateCheckOutline()
    {
        if (checkOutlineObj != null || checkLeverMeshFilter == null || checkLeverTransform == null) return;
        checkOutlineObj = new GameObject("CheckLeverOutline");
        checkOutlineObj.transform.SetParent(checkLeverTransform);
        checkOutlineObj.transform.localPosition = Vector3.zero;
        checkOutlineObj.transform.localRotation = Quaternion.identity;
        checkOutlineObj.transform.localScale = Vector3.one * (1f + checkOutlineWidth);
        checkOutlineObj.AddComponent<MeshFilter>().mesh = checkLeverMeshFilter.mesh;
        MeshRenderer mr = checkOutlineObj.AddComponent<MeshRenderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = checkHighlightColor;
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