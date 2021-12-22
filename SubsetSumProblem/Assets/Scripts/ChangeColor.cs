using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColor : MonoBehaviour
{
    public void Green()
    {
        var renderer = GetComponent<Renderer>();
        renderer.material.SetColor("_Color", Color.green);
    }

    public void Red()
    {
        var renderer = GetComponent<Renderer>();
        renderer.material.SetColor("_Color", Color.red);
    }
}
