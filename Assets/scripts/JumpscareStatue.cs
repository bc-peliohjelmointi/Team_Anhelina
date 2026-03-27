using UnityEngine;
using System.Collections;

public class JumpscareStatue : MonoBehaviour
{
    public AudioClip jumpscareClip;
    public GameObject statue; // drag statue prefab/scene object vào
    private AudioSource audioSource;
    private bool triggered = false;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = jumpscareClip;
        statue.SetActive(false); // đảm bảo statue ban đầu ẩn
    }

    void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            audioSource.Play();
            StartCoroutine(ShowStatueTemporarily());
            triggered = true;
        }
    }

    IEnumerator ShowStatueTemporarily()
    {
        statue.SetActive(true);
        yield return new WaitForSeconds(6f); // hiển thị 3 giây
        statue.SetActive(false);
    }
}