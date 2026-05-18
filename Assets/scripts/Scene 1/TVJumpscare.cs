using UnityEngine;
using System.Collections;

public class TVJumpscare : MonoBehaviour
{
    [Header("References")]
    public GameObject tvScreen;
    public Light buttonLight;
    public AudioSource audioSource;
    public GameObject ghost;

    [Header("Monster")]
    public GameObject monster;
    public AudioSource monsterSound;
    public float monsterSoundDuration = 3.5f;
    public float monsterSpawnY = 21.1f;
    public float monsterSpawnDistance = 1.5f;

    [Header("Player")]
    public Transform playerObject;
    public Transform playerCamera;
    public PlayerMovement playerMovement;

    [Header("Settings")]
    public float delay = 1.5f;
    public float ghostDuration = 7f;
    public float monsterDelay = 5f;
    public float rotateDuration = 0.3f;
    public float freezeDuration = 2f;

    private bool hasTriggered = false;

    void OnMouseDown()
    {
        if (hasTriggered) return;
        hasTriggered = true;
        tvScreen.SetActive(false);
        buttonLight.enabled = false;
        Invoke(nameof(PlaySound), delay);
    }

    void PlaySound()
    {
        audioSource.Play();
        ghost.SetActive(true);
        Invoke(nameof(HideGhost), ghostDuration);
    }

    void HideGhost()
    {
        ghost.SetActive(false);
        StartCoroutine(MonsterRoutine());
    }

    IEnumerator MonsterRoutine()
    {
        yield return new WaitForSeconds(monsterDelay);

        // lock camera trước khi quay
        if (playerMovement != null)
            playerMovement.LockCamera();

        yield return StartCoroutine(RotatePlayer180());

        Vector3 spawnPos = playerObject.position + playerObject.forward * monsterSpawnDistance + playerObject.right * -2f;
        spawnPos.y = monsterSpawnY;
        monster.transform.position = spawnPos;
        monster.transform.LookAt(new Vector3(playerObject.position.x, monsterSpawnY, playerObject.position.z));
        monster.transform.Rotate(0, 3f, 0);

        monster.SetActive(true);
        if (monsterSound != null)
            monsterSound.Play();

        // lock player
        if (playerMovement != null)
            playerMovement.enabled = false;

        yield return new WaitForSeconds(freezeDuration);

        // unlock player
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
            playerMovement.UnlockCamera();
            playerMovement.xRotation = -10f;

            playerMovement.targetCameraHeight = playerMovement.playerCamera.localPosition.y;
            playerMovement.currentCameraHeight = playerMovement.playerCamera.localPosition.y;
        }

        yield return new WaitForSeconds(monsterSoundDuration);

        if (monsterSound != null)
            monsterSound.Stop();

        monster.SetActive(false);
    }

    IEnumerator RotatePlayer180()
    {
        Quaternion startRot = playerObject.rotation;
        Quaternion endRot = playerObject.rotation * Quaternion.Euler(0, 180f, 0);

        Quaternion startCamRot = playerCamera.localRotation;
        Quaternion endCamRot = Quaternion.Euler(-10f, 0f, 0f); 

        float timer = 0f;

        while (timer < rotateDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, timer / rotateDuration);
            playerObject.rotation = Quaternion.Slerp(startRot, endRot, t);
            playerCamera.localRotation = Quaternion.Slerp(startCamRot, endCamRot, t);
            yield return null;
        }

        playerObject.rotation = endRot;
        playerCamera.localRotation = endCamRot;

        if (playerMovement != null)
        {
            playerMovement.freezeCameraHeight = true;
            playerMovement.currentCameraHeight = playerCamera.localPosition.y;
            playerMovement.targetCameraHeight = playerCamera.localPosition.y;
        }
    }
}