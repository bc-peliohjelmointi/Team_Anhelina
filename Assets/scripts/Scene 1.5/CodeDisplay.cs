using UnityEngine;
using UnityEngine.UI;
// attach to the monitor object in the scene
// grabs code from SceneCodeManager and shows it on a World Space Canvas
// player walks up to the monitor, reads the code, goes to the computer
// make sure SceneCodeManager is in the scene or this will show "????????"
public class CodeDisplay : MonoBehaviour
{
    // the Text component on the canvas attached to this monitor
    public Text codeText;
    // text prefix shown before the digits
    public string prefix = "ACCESS CODE: ";
    void Start()
    {
        if (SceneCodeManager.Instance != null && codeText != null)
            codeText.text = prefix + SceneCodeManager.Instance.GetCode();
    }
}