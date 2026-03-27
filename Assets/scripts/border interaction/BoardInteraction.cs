using System.Collections;
using UnityEngine;
public class BoardInteraction : MonoBehaviour
{
    [Header("Player")]
    public MonoBehaviour playerController;
    public Camera playerCamera;
    [Header("Camera View")]
    public Transform cameraPoint;
    public float viewTime = 60f;
    [Header("Dialogue")]
    public DialoguePlayer dialogue;
    [Header("Map")]
    public Renderer mapRenderer;
    public Texture[] missionTextures;
    public MissionSystem missionSystem;
    private bool playerNear = false;
    private bool isViewing = false;
    private Vector3 oldCamPos;
    private Quaternion oldCamRot;
    public float fadeDuration = 1f;
    void Start()
    {
        InteractionHint.instance.Hide();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (!isViewing)
                EnterView();
            else
            {
                if (dialogue != null)
                    dialogue.Stop();
                ExitView();
            }
        }
    }
    IEnumerator FadeTexture(Texture newTexture)
    {
        Material mat = mapRenderer.material;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = 1f - (t / fadeDuration);
            mat.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
        mat.mainTexture = newTexture;
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = t / fadeDuration;
            mat.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
    }
    void EnterView()
    {
        isViewing = true;
        InteractionHint.instance.Hide();
        oldCamPos = playerCamera.transform.position;
        oldCamRot = playerCamera.transform.rotation;
        if (playerController != null)
            playerController.enabled = false;
        playerCamera.transform.position = cameraPoint.position;
        playerCamera.transform.rotation = cameraPoint.rotation;
        UpdateMapTexture();
        if (dialogue != null)
            dialogue.Play();
        StartCoroutine(ViewTimer());
    }
    IEnumerator ViewTimer()
    {
        float timer = 0f;
        while (timer < viewTime)
        {
            if (dialogue != null && !dialogue.IsPlaying())
                break;
            timer += Time.deltaTime;
            yield return null;
        }
        ExitView();
    }
    void ExitView()
    {
        if (!isViewing) return;
        isViewing = false;
        if (playerController != null)
            playerController.enabled = true;
        playerCamera.transform.position = oldCamPos;
        playerCamera.transform.rotation = oldCamRot;
        if (playerNear)
            InteractionHint.instance.Show("Press M to view map");
    }
    void UpdateMapTexture()
    {
        if (missionSystem == null || mapRenderer == null) return;
        int mission = missionSystem.GetCurrentMission();
        if (mission >= 0 && mission < missionTextures.Length)
            StartCoroutine(FadeTexture(missionTextures[mission]));
    }
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerNear = true;
        if (!isViewing)
            InteractionHint.instance.Show("Press M to view map");
    }
    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerNear = false;
        InteractionHint.instance.Hide();
    }
}