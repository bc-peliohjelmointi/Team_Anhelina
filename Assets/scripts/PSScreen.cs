using UnityEngine;
using System.Collections;

public class PSScreen : MonoBehaviour
{
    public Renderer screenRenderer;
    public Material glitchMaterial;
    public Material offScreenMaterial;
    public float glitchShowDelay = 0f;
    public float menuShowDelay = 1f;

    [Header("Text Settings")]
    public Color textColor = Color.green;
    public float textScale = 0.001f;
    public Vector3 textContainerPosition = new Vector3(0, 0, 0.05f);
    public Vector3 textContainerRotation = new Vector3(0, 0, 0);
    public float horizontalOffset = 0f;
    public float verticalOffset = 0f;
    public float textSize = 100f;
    public float lineSpacing = 20f;
    public float checkmarkOffsetX = 60f;
    public float textStartY = 30f;

    [Header("Highlight Settings")]
    public float highlightWidth = 140f;
    public float highlightHeight = 15f;
    public Color highlightColor = new Color(0f, 1f, 0f, 0.3f);

    [Header("Error Text Settings")]
    public Color errorColor = Color.red;
    public float errorTextSizeMultiplier = 1.5f;

    private GameObject textContainer;
    private GameObject[] menuTexts = new GameObject[3];
    private GameObject[] checkmarks = new GameObject[3];
    private GameObject highlightQuad;
    private GameObject errorTextObj;

    private bool isOn = false;
    private bool isTextVisible = false;

    void Start()
    {
        if (screenRenderer == null)
        {
            screenRenderer = GetComponent<Renderer>();
        }

        if (screenRenderer != null && offScreenMaterial != null)
        {
            screenRenderer.material = offScreenMaterial;
        }

        CreateTextUI();
    }

    void CreateTextUI()
    {
        textContainer = new GameObject("TextContainer");
        textContainer.transform.SetParent(transform);
        textContainer.transform.localPosition = textContainerPosition;
        textContainer.transform.localRotation = Quaternion.Euler(textContainerRotation);
        textContainer.transform.localScale = Vector3.one;

        string[] options = new string[]
        {
            "Turn on first episode",
            "Turn on second episode",
            "Turn on third episode"
        };

        for (int i = 0; i < 3; i++)
        {
            float yPosition = textStartY - i * lineSpacing + verticalOffset;

            Vector3 textPosition = new Vector3(-checkmarkOffsetX + horizontalOffset, yPosition, 1);
            menuTexts[i] = CreateText(options[i], textPosition, TextAnchor.MiddleLeft);

            Vector3 checkPosition = new Vector3(checkmarkOffsetX + horizontalOffset, yPosition, 1);
            checkmarks[i] = CreateText("V", checkPosition, TextAnchor.MiddleCenter);
            checkmarks[i].SetActive(false);
        }

        Vector3 highlightPosition = new Vector3(0 + horizontalOffset, textStartY + verticalOffset, 0.5f);
        highlightQuad = CreateQuad(highlightPosition, highlightWidth, highlightHeight);

        Vector3 errorPosition = new Vector3(0 + horizontalOffset, 0 + verticalOffset, 1);
        errorTextObj = CreateText("TURN ON TV", errorPosition, TextAnchor.MiddleCenter);
        TextMesh errorTM = errorTextObj.GetComponent<TextMesh>();
        errorTM.fontSize = (int)(textSize * errorTextSizeMultiplier);
        errorTextObj.GetComponent<MeshRenderer>().material.color = errorColor;
        errorTextObj.SetActive(false);

        HideText();
    }

    GameObject CreateText(string text, Vector3 position, TextAnchor anchor)
    {
        GameObject obj = new GameObject("Text_" + text.Replace(" ", "_"));
        obj.transform.SetParent(textContainer.transform);
        obj.transform.localPosition = position * textScale;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one * textScale;

        TextMesh tm = obj.AddComponent<TextMesh>();
        tm.text = text;
        tm.fontSize = (int)textSize;
        tm.characterSize = 1f;
        tm.anchor = anchor;
        tm.alignment = (anchor == TextAnchor.MiddleCenter) ? TextAlignment.Center : TextAlignment.Left;
        tm.color = textColor;

        MeshRenderer mr = obj.AddComponent<MeshRenderer>();
        Material textMaterial = new Material(Shader.Find("GUI/Text Shader"));
        textMaterial.color = textColor;
        mr.material = textMaterial;

        obj.layer = gameObject.layer;

        return obj;
    }

    GameObject CreateQuad(Vector3 position, float width, float height)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        obj.name = "Highlight";
        obj.transform.SetParent(textContainer.transform);
        obj.transform.localPosition = position * textScale;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = new Vector3(width, height, 1) * textScale;

        Collider collider = obj.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        MeshRenderer mr = obj.GetComponent<MeshRenderer>();
        Material quadMaterial = new Material(Shader.Find("Unlit/Transparent"));
        quadMaterial.color = highlightColor;
        mr.material = quadMaterial;

        obj.layer = gameObject.layer;

        return obj;
    }

    public void TurnOn()
    {
        isOn = true;
        StopAllCoroutines();
        StartCoroutine(TurnOnSequence());
    }

    public void TurnOff()
    {
        isOn = false;
        StopAllCoroutines();

        HideText();

        if (screenRenderer != null && offScreenMaterial != null)
        {
            screenRenderer.material = offScreenMaterial;
        }
    }

    IEnumerator TurnOnSequence()
    {
        yield return new WaitForSeconds(glitchShowDelay);

        if (screenRenderer != null && glitchMaterial != null && isOn)
        {
            screenRenderer.material = glitchMaterial;
        }

        yield return new WaitForSeconds(menuShowDelay);

        if (isOn)
        {
            ShowText();
        }
    }

    void ShowText()
    {
        isTextVisible = true;

        foreach (var text in menuTexts)
        {
            if (text != null)
            {
                text.SetActive(true);
            }
        }

        if (highlightQuad != null)
        {
            highlightQuad.SetActive(true);
        }
    }

    void HideText()
    {
        isTextVisible = false;

        foreach (var text in menuTexts)
        {
            if (text != null)
            {
                text.SetActive(false);
            }
        }

        foreach (var check in checkmarks)
        {
            if (check != null)
            {
                check.SetActive(false);
            }
        }

        if (highlightQuad != null)
        {
            highlightQuad.SetActive(false);
        }

        if (errorTextObj != null)
        {
            errorTextObj.SetActive(false);
        }
    }

    public void UpdateSelection(int index)
    {
        if (highlightQuad != null && index >= 0 && index < 3)
        {
            float yPosition = textStartY - index * lineSpacing + verticalOffset;
            Vector3 newPosition = new Vector3(0 + horizontalOffset, yPosition, 0.5f);
            highlightQuad.transform.localPosition = newPosition * textScale;
        }
    }

    public void ShowCheckmark(int index)
    {
        if (index >= 0 && index < checkmarks.Length && checkmarks[index] != null)
        {
            checkmarks[index].SetActive(true);
        }
    }

    public void HideCheckmark(int index)
    {
        if (index >= 0 && index < checkmarks.Length && checkmarks[index] != null)
        {
            checkmarks[index].SetActive(false);
        }
    }

    public void ShowError(string message)
    {
        if (errorTextObj != null)
        {
            TextMesh errorTM = errorTextObj.GetComponent<TextMesh>();
            if (errorTM != null)
            {
                errorTM.text = message;
            }
            errorTextObj.SetActive(true);
        }
    }

    public void HideError()
    {
        if (errorTextObj != null)
        {
            errorTextObj.SetActive(false);
        }
    }

    public bool IsOn()
    {
        return isOn;
    }

    public bool IsTextVisible()
    {
        return isTextVisible;
    }

    void OnValidate()
    {
        if (textContainer != null)
        {
            textContainer.transform.localPosition = textContainerPosition;
            textContainer.transform.localRotation = Quaternion.Euler(textContainerRotation);

            if (menuTexts != null && menuTexts.Length > 0)
            {
                for (int i = 0; i < menuTexts.Length; i++)
                {
                    if (menuTexts[i] != null)
                    {
                        float yPosition = textStartY - i * lineSpacing + verticalOffset;
                        Vector3 textPosition = new Vector3(-checkmarkOffsetX + horizontalOffset, yPosition, 1);
                        menuTexts[i].transform.localPosition = textPosition * textScale;

                        TextMesh tm = menuTexts[i].GetComponent<TextMesh>();
                        if (tm != null)
                        {
                            tm.fontSize = (int)textSize;
                            tm.color = textColor;
                        }
                    }

                    if (checkmarks[i] != null)
                    {
                        float yPosition = textStartY - i * lineSpacing + verticalOffset;
                        Vector3 checkPosition = new Vector3(checkmarkOffsetX + horizontalOffset, yPosition, 1);
                        checkmarks[i].transform.localPosition = checkPosition * textScale;

                        TextMesh tm = checkmarks[i].GetComponent<TextMesh>();
                        if (tm != null)
                        {
                            tm.fontSize = (int)textSize;
                            tm.color = textColor;
                        }
                    }
                }
            }

            if (highlightQuad != null)
            {
                Vector3 highlightPosition = new Vector3(0 + horizontalOffset, textStartY + verticalOffset, 0.5f);
                highlightQuad.transform.localPosition = highlightPosition * textScale;
                highlightQuad.transform.localScale = new Vector3(highlightWidth, highlightHeight, 1) * textScale;

                MeshRenderer mr = highlightQuad.GetComponent<MeshRenderer>();
                if (mr != null && mr.material != null)
                {
                    mr.material.color = highlightColor;
                }
            }

            if (errorTextObj != null)
            {
                Vector3 errorPosition = new Vector3(0 + horizontalOffset, 0 + verticalOffset, 1);
                errorTextObj.transform.localPosition = errorPosition * textScale;

                TextMesh errorTM = errorTextObj.GetComponent<TextMesh>();
                if (errorTM != null)
                {
                    errorTM.fontSize = (int)(textSize * errorTextSizeMultiplier);
                }

                MeshRenderer mr = errorTextObj.GetComponent<MeshRenderer>();
                if (mr != null && mr.material != null)
                {
                    mr.material.color = errorColor;
                }
            }
        }
    }
}