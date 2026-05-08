using UnityEngine;
using UnityEngine.UI;
// ============================================================
// PSDesktopIcon.cs
// ============================================================
// Put this on each icon GameObject on the desktop
// ObjectDragRay detects PSDesktopIcon via GetComponent
// and calls Press() when player clicks LMB
//
// Each icon needs:
// - A Collider (BoxCollider) for the raycast
// - An Image or Renderer for the icon visual
// - This script
// ============================================================
public class PSDesktopIcon : MonoBehaviour
{
    [Header("Icon Info")]
    public string iconName = "Icon";

    [Header("Visual")]
    // Image component for UI icons
    public Image iconImage;
    // OR Renderer for 3D quad icons
    public Renderer iconRenderer;
    // sprite shown normally
    public Sprite normalSprite;
    // sprite shown when hovered
    public Sprite hoverSprite;

    [Header("Hover Highlight")]
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(0.7f, 0.9f, 1f, 1f);

    [Header("Label")]
    public Text iconLabel;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;
    public AudioClip hoverSound;
    public float soundVolume = 0.5f;

    [Header("Animation")]
    public float hoverScaleBoost = 1.1f;
    public float animSpeed = 8f;

    private PSDesktop desktop;
    private bool isHovered = false;
    private Vector3 originalScale;

    void Start()
    {
        desktop = GetComponentInParent<PSDesktop>();
        if (desktop == null) desktop = PSDesktop.Instance;
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        originalScale = transform.localScale;
    }

    void Update()
    {
        // animate scale on hover
        float targetScale = isHovered ? hoverScaleBoost : 1f;
        transform.localScale = Vector3.Lerp(transform.localScale,
            originalScale * targetScale, Time.deltaTime * animSpeed);
    }

    // called by ObjectDragRay (via AuraHighlight system or direct check)
    public void Press()
    {
        if (clickSound != null) audioSource.PlayOneShot(clickSound, soundVolume);
        if (desktop != null) desktop.OnIconClicked(this);
    }

    // called when player looks at this icon
    public void SetHover(bool on)
    {
        isHovered = on;
        if (on && !isHovered && hoverSound != null)
            audioSource.PlayOneShot(hoverSound, soundVolume * 0.5f);
        Color c = on ? hoverColor : normalColor;
        if (iconImage != null) iconImage.color = c;
        if (iconRenderer != null) iconRenderer.material.color = c;
        if (on && hoverSprite != null && iconImage != null) iconImage.sprite = hoverSprite;
        else if (!on && normalSprite != null && iconImage != null) iconImage.sprite = normalSprite;
    }
}