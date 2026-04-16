using UnityEngine;
using System.Collections;
// controls row 1 of the lever puzzle - 8 small levers
// correctCombination is the answer, false = down, true = up
// lights go green progressively from left to right as player gets them right
// when all correct and player holds check lever - validation animation plays
// after animation finishes, tells MainControlPanel to light up indicator 1
public class PuzzleLevel1 : MonoBehaviour
{
    // all 8 levers in row 1, drag them in order left to right
    public Lever lever1, lever2, lever3, lever4, lever5, lever6, lever7, lever8;
    // the correct combination for this row, change to whatever you want
    public bool[] correctCombination = new bool[8] { false, true, true, true, false, true, false, true };
    // the physical check lever transform that animates up and down
    public Transform checkLeverTransform;
    // needed to create the outline highlight effect on the check lever
    public MeshFilter checkLeverMeshFilter;
    // rotation when check lever is in rest position
    public Vector3 checkDownRotation = new Vector3(45f, 0f, 0f);
    // rotation when player is holding check lever
    public Vector3 checkUpRotation = new Vector3(-45f, 0f, 0f);
    public float checkLeverSpeed = 5f;

    // total time the validation success animation takes in seconds
    public float validationHoldTime = 5f;
    public Color checkHighlightColor = Color.yellow;
    public float checkOutlineWidth = 0.02f;

    public AudioSource audioSource;
    // sound when combination is correct and validated
    public AudioClip correctSound;
    // sound when player releases check lever before validation finishes
    public AudioClip wrongSound;
    public float soundVolume = 0.5f;

    // reference to the main panel so we can notify it when this row is done
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
        // set initial light states based on combination
        UpdateAllLights();
    }

    void Update()
    {
        // smoothly animate the check lever transform
        if (checkLeverTransform != null)
            checkLeverTransform.localRotation = Quaternion.Lerp(
                checkLeverTransform.localRotation, checkTargetRotation, Time.deltaTime * checkLeverSpeed);
    }

    // called by CheckLeverComponent when player presses and holds
    public void PullCheckLever()
    {
        if (isSolved || isValidating) return;
        checkTargetRotation = Quaternion.Euler(checkUpRotation);
        // only start validation sequence if combination is actually correct
        if (!CheckIfCorrect()) return;
        if (validationCoroutine != null) StopCoroutine(validationCoroutine);
        validationCoroutine = StartCoroutine(ValidationSequence());
    }

    // called when player lets go of check lever before it finishes
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
        // reset all lights to red first, then animate them green one by one
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
        // tell the control panel row 1 is done
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

    // called by each Lever when toggled so lights update in real time
    public void OnLeverChanged()
    {
        if (!isSolved && !isValidating) UpdateAllLights();
    }

    // lights go green progressively - stops at first wrong lever
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