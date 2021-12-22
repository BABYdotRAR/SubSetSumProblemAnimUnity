using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class ButtonFunctions : MonoBehaviour
{
    public TMP_InputField addNumText;
    public TMP_Text arrayText;
    public TMP_Text warningText;
    public TMP_InputField setSumText;
    public CreateTable table;
    public AlgoAnimation anim;
    public GameObject parent;
    public GameObject cubePrefab;
    public GameObject cubesParent;

    public void AddToArray()
    {
        if(GlobalValues.instance.A.Count >= 7)
        {
            warningText.text = "Advertencia: se restringió a solo 7 elementos en el arreglo para no demorar tanto la animación.";
            return;
        }

        try
        {
            int num = int.Parse(addNumText.text);
            GlobalValues.instance.AddNumber(num);

            GameObject cube = Instantiate(cubePrefab);
            cube.transform.SetParent(cubesParent.transform, false);
            cube.GetComponent<EditableText>().setText(num.ToString() + "L");
            cube.GetComponent<DeleteNumber>().SetNum(num);
        }
        catch(Exception e)
        {
            warningText.text = "Advertencia: Ingrese un número entero.";
        }
    }

    public void SetSum()
    {
        int sum = int.Parse(setSumText.text);
        GlobalValues.instance.sum = sum;
    }

    public void init()
    {
        try
        {
            int n = int.Parse(setSumText.text);
            if (n > 10)
            {
                warningText.text = "Advertencia: se restringió la suma a 10 para no demorar tanto en la animación";
                return;
            }
            SetSum();
            anim.StartAnimation();
            parent.SetActive(false);
        }
        catch(Exception e)
        {
            warningText.text = "Advertencia: introduzca una cantidad entera de árboles";
            return;
        }
    }

    public void Continue()
    {
        anim.ContinueButton();
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }
}
