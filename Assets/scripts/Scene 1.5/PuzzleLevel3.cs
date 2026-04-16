using UnityEngine;
using System.Collections;
// row 3 is mixed - 4 small levers and 2 double levers
// combination order: smallLever1, doubleLever1, smallLever2, smallLever3, doubleLever2, smallLever4
// same validation logic as rows 1 and 2
public class PuzzleLevel3 : MonoBehaviour
{
    // 4 small levers
    public Lever smallLever1, smallLever2, smallLever3, smallLever4;
    // 2 double levers
    public DoubleLever doubleLever1, doubleLever2;
    // combination order: small1 double1 small2 small3 double2 small4
    public bool[] correctCombination = new bool[6] { false, true, false, true, false, true };
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
    private GameObject checkOutlineObj;
    private Coroutine validationCoroutine;

    void Start()
    {
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
        // light them up one by one in order: small1, double1, small2, small3, double2, small4
        float delay = (validationHoldTime - 0.5f) / 6f;
        if (smallLever1 != null) { smallLever1.SetLightGreen(true); yield return new WaitForSeconds(delay); }
        if (doubleLever1 != null) { doubleLever1.SetTopLightGreen(true); doubleLever1.SetBottomLightGreen(true); yield return new WaitForSeconds(delay); }
        if (smallLever2 != null) { smallLever2.SetLightGreen(true); yield return new WaitForSeconds(delay); }
        if (smallLever3 != null) { smallLever3.SetLightGreen(true); yield return new WaitForSeconds(delay); }
        if (doubleLever2 != null) { doubleLever2.SetTopLightGreen(true); doubleLever2.SetBottomLightGreen(true); yield return new WaitForSeconds(delay); }
        if (smallLever4 != null) { smallLever4.SetLightGreen(true); }
        isSolved = true;
        isValidating = false;
        if (correctSound != null) audioSource.PlayOneShot(correctSound, soundVolume);
        if (controlPanel != null) controlPanel.NotifyLevelSolved();
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

    public void OnLeverChanged()
    {
        if (!isSolved && !isValidating) UpdateAllLights();
    }

    // written out explicitly to avoid local function syntax issues in older Unity
    void UpdateAllLights()
    {
        bool prev = true;

        if (smallLever1 != null)
        {
            bool c = smallLever1.isUp == correctCombination[0];
            smallLever1.SetLightGreen(c && prev);
            if (!c) prev = false;
        }
        if (doubleLever1 != null)
        {
            bool c = doubleLever1.isUp == correctCombination[1];
            doubleLever1.SetTopLightGreen(c && prev);
            doubleLever1.SetBottomLightGreen(c && prev);
            if (!c) prev = false;
        }
        if (smallLever2 != null)
        {
            bool c = smallLever2.isUp == correctCombination[2];
            smallLever2.SetLightGreen(c && prev);
            if (!c) prev = false;
        }
        if (smallLever3 != null)
        {
            bool c = smallLever3.isUp == correctCombination[3];
            smallLever3.SetLightGreen(c && prev);
            if (!c) prev = false;
        }
        if (doubleLever2 != null)
        {
            bool c = doubleLever2.isUp == correctCombination[4];
            doubleLever2.SetTopLightGreen(c && prev);
            doubleLever2.SetBottomLightGreen(c && prev);
            if (!c) prev = false;
        }
        if (smallLever4 != null)
        {
            bool c = smallLever4.isUp == correctCombination[5];
            smallLever4.SetLightGreen(c && prev);
        }
    }

    void SetAllLights(bool green)
    {
        if (smallLever1 != null) smallLever1.SetLightGreen(green);
        if (doubleLever1 != null) { doubleLever1.SetTopLightGreen(green); doubleLever1.SetBottomLightGreen(green); }
        if (smallLever2 != null) smallLever2.SetLightGreen(green);
        if (smallLever3 != null) smallLever3.SetLightGreen(green);
        if (doubleLever2 != null) { doubleLever2.SetTopLightGreen(green); doubleLever2.SetBottomLightGreen(green); }
        if (smallLever4 != null) smallLever4.SetLightGreen(green);
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