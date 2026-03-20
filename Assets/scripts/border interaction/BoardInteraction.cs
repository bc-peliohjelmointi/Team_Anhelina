using System.Collections;
using UnityEngine;

public class BoardInteraction : MonoBehaviour
{
    [Header("Hint")]
    public GameObject interactHint;

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
        if (interactHint != null)
            interactHint.SetActive(false);
    }

    void Update()
    {
        // Вход в режим просмотра
        if (playerNear && Input.GetKeyDown(KeyCode.E) && !isViewing)
        {
            EnterView();
        }
        // Выход по E
        else if (isViewing && Input.GetKeyDown(KeyCode.E))
        {
            ExitView();
        }
    }
    IEnumerator FadeTexture(Texture newTexture)
    {
        Material mat = mapRenderer.material;

        float t = 0f;

        // затемняем
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = 1f - (t / fadeDuration);

            mat.color = new Color(1, 1, 1, alpha);
            yield return null;
        }

        // меняем текстуру
        mat.mainTexture = newTexture;

        t = 0f;

        // осветляем обратно
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

        // скрываем подсказку
        if (interactHint != null)
            interactHint.SetActive(false);

        // сохраняем камеру
        oldCamPos = playerCamera.transform.position;
        oldCamRot = playerCamera.transform.rotation;

        // выключаем управление
        if (playerController != null)
            playerController.enabled = false;

        // ставим камеру на точку
        playerCamera.transform.position = cameraPoint.position;
        playerCamera.transform.rotation = cameraPoint.rotation;

        // меняем карту
        UpdateMapTexture();

        // запускаем диалог
        if (dialogue != null)
            dialogue.Play();

        // запускаем таймер (1 минута)
        StartCoroutine(ViewTimer());
    }

    IEnumerator ViewTimer()
    {
        float timer = 0f;

        while (timer < viewTime)
        {
            // если диалог закончился — можно выйти раньше (опционально)
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

        // возвращаем управление
        if (playerController != null)
            playerController.enabled = true;

        // возвращаем камеру
        playerCamera.transform.position = oldCamPos;
        playerCamera.transform.rotation = oldCamRot;

        // показываем подсказку если игрок рядом
        if (playerNear && interactHint != null)
            interactHint.SetActive(true);
    }

    void UpdateMapTexture()
    {
        if (missionSystem == null || mapRenderer == null) return;

        int mission = missionSystem.GetCurrentMission();

        if (mission >= 0 && mission < missionTextures.Length)
        {
            StartCoroutine(FadeTexture(missionTextures[mission]));
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerNear = true;

        if (!isViewing && interactHint != null)
            interactHint.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerNear = false;

        if (interactHint != null)
            interactHint.SetActive(false);
    }
}