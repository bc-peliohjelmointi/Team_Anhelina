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
    private bool checkLeverUp = false;
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

    public void CheckCombination()
    {
        if (isSolved) return;

        bool isCorrect = CheckIfCorrect();

        if (isCorrect)
        {
            isSolved = true;
            checkLeverUp = true;
            checkTargetRotation = Quaternion.Euler(checkUpRotation);

            if (correctSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(correctSound, soundVolume);
            }

            SetAllLightsGreen();
        }
        else
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

        for (int i = 0; i < singleLevers.Length; i++)
        {
            if (singleLevers[i] == null) continue;

            bool isCorrect = index < correctCombination.Length &&
                           singleLevers[i].isUp == correctCombination[index];

            singleLevers[i].SetLightGreen(isCorrect);
            index++;
        }

        for (int i = 0; i < doubleLevers.Length; i++)
        {
            if (doubleLevers[i] == null) continue;

            bool isCorrect = index < correctCombination.Length &&
                           doubleLevers[i].isUp == correctCombination[index];

            doubleLevers[i].SetTopLightGreen(isCorrect);
            doubleLevers[i].SetBottomLightGreen(isCorrect);
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