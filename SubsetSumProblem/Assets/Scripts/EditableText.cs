using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EditableText : MonoBehaviour
{
    public TMP_Text txt;

    public void setText(string text)
    {
        txt.text = text;
    }
}
