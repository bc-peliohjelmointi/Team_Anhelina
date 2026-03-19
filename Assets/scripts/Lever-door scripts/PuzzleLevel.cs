using UnityEngine;

public class PuzzleLevel : MonoBehaviour
{
    [Header("Levers")]
    public Lever[] singleLevers;
    public DoubleLever[] doubleLevers;

    [Header("Combination")]
    public bool[] correctCombination;

    [Header("Check Lever")]
    public Transform checkLever;
    public Vector3 checkDownRotation = new Vector3(45, 0, 0);
    public Vector3 checkUpRotation = new Vector3(-45, 0, 0);
    public float checkLeverSpeed = 5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public float soundVolume = 0.5f;

    private bool isSolved = false;
    private Quaternion checkTargetRotation;

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
        checkTargetRotation = Quaternion.Euler(checkUpRotation);
    }

    public void ReleaseCheckLever()
    {
        checkTargetRotation = Quaternion.Euler(checkDownRotation);

        bool isCorrect = CheckIfCorrect();

        if (isCorrect && !isSolved)
        {
            isSolved = true;

            if (correctSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(correctSound, soundVolume);
            }

            SetAllLightsGreen();
        }
        else if (!isCorrect)
        {
            if (wrongSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(wrongSound, soundVolume);
            }

            UpdateAllLights();
        }
    }

    bool CheckIfCorrect()
    {
        int index = 0;

        for (int i = 0; i < singleLevers.Length; i++)
        {
            if (singleLevers[i] == null) continue;

            if (index >= correctCombination.Length) return false;

            if (singleLevers[i].isUp != correctCombination[index])
            {
                return false;
            }
            index++;
        }

        for (int i = 0; i < doubleLevers.Length; i++)
        {
            if (doubleLevers[i] == null) continue;

            if (index >= correctCombination.Length) return false;

            if (doubleLevers[i].isUp != correctCombination[index])
            {
                return false;
            }
            index++;
        }

        return true;
    }

    void UpdateAllLights()
    {
        int index = 0;
        bool previousCorrect = true;

        for (int i = 0; i < singleLevers.Length; i++)
        {
            if (singleLevers[i] == null) continue;

            bool currentCorrect = index < correctCombination.Length &&
                                singleLevers[i].isUp == correctCombination[index];

            bool shouldBeGreen = currentCorrect && previousCorrect;

            singleLevers[i].SetLightGreen(shouldBeGreen);

            if (!currentCorrect)
            {
                previousCorrect = false;
            }

            index++;
        }

        for (int i = 0; i < doubleLevers.Length; i++)
        {
            if (doubleLevers[i] == null) continue;

            bool currentCorrect = index < correctCombination.Length &&
                                doubleLevers[i].isUp == correctCombination[index];

            bool shouldBeGreen = currentCorrect && previousCorrect;

            doubleLevers[i].SetTopLightGreen(shouldBeGreen);
            doubleLevers[i].SetBottomLightGreen(shouldBeGreen);

            if (!currentCorrect)
            {
                previousCorrect = false;
            }

            index++;
        }
    }

    void SetAllLightsGreen()
    {
        foreach (Lever lever in singleLevers)
        {
            if (lever != null)
            {
                lever.SetLightGreen(true);
            }
        }

        foreach (DoubleLever lever in doubleLevers)
        {
            if (lever != null)
            {
                lever.SetTopLightGreen(true);
                lever.SetBottomLightGreen(true);
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
}
