using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AlgoAnimation : MonoBehaviour
{
    [HideInInspector] public AnimStates currentState = AnimStates.EXPLANATION_0;
    Dictionary<AnimStates, Func<int>> states = new Dictionary<AnimStates, Func<int>>();

    public CanvasGroup equivCanvasGroup;
    public CanvasGroup btnCanvasGroup;
    public float fadeSpeed;
    public float enableCooldown;
    public GameObject treeNumberPrefab;
    public GameObject waterCubeNumPrefab;
    public GameObject treePrefab;
    public GameObject rootPrefab;
    public Transform columnIndicator;
    public Transform rowIndicator;
    public Transform[] spawnPos;
    public Transform minBounds;
    public Transform maxBounds;
    public TMP_Text description;
    public float minDist;
    public GameObject endPanel;
    public TMP_Text endText;

    float xStep, zStep, currentX, currentZ, xMin, zMin, xMax, zMax;
    int sum, length, _i, _j;
    bool[,] DP;
    Vector3 columnInitialPos, rowInitalPos;

    string green = "086972";

    private void Start()
    {
        columnInitialPos = columnIndicator.position;
        rowInitalPos = rowIndicator.position;

        states.Add(AnimStates.EXPLANATION_1, Explanation1);
        states.Add(AnimStates.CREATE_COLUMNS, CreateColumns);
        states.Add(AnimStates.CREATE_ROWS, CreateRows);
        states.Add(AnimStates.FILL_FIRST_COLUMN, Fill1stColumn);
        states.Add(AnimStates.FILL_FIRST_ROW, Fill1stRow);
        states.Add(AnimStates.FILL_AT, FillAt);
        states.Add(AnimStates.FILL_AT_COMP, FillComp);
        states.Add(AnimStates.FILL_AT_RESULT, FillResult);
    }

    public void StartAnimation()
    {
        SetSumAndLength();
        DP = new bool[length + 1, sum + 1];
        CalculateSteps();
        StartExplanation();
    }

    #region BtnFunctionality
    public void ContinueButton()
    {
        states[currentState].Invoke();
        StartCoroutine(FadeOutBtn());
    }

    void EnableButton()
    {
        StartCoroutine(FadeInBtn());
    }

    IEnumerator FadeInBtn()
    {
        while (btnCanvasGroup.alpha < 1f)
        {
            btnCanvasGroup.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }
        btnCanvasGroup.blocksRaycasts = true;
    }

    IEnumerator FadeOutBtn()
    {
        btnCanvasGroup.blocksRaycasts = false;
        while(btnCanvasGroup.alpha > 0.1f)
        {
            btnCanvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
    }
    #endregion

    #region Explanation
    void StartExplanation()
    {
        description.text = "Supongamos que para hacer crecer a un árbol necesitamos un cubo de 1L de agua";
        StartCoroutine(FadeInEquiv());
        EnableButton();
        currentState = AnimStates.EXPLANATION_1;
    }

    public int Explanation1()
    {
        StartCoroutine(FadeOutEquiv());
        description.text = "Así que veamos si en nuestro contenedor con <#" + green + ">" + length.ToString() + " cubos de agua</color> de distinta capacidad hay algún subconjunto cuya suma sea igual a la cantidad de agua necesaria para hacer crecer a <#" + green + ">" + sum.ToString() + " árboles.</color>";
        Invoke("EnableButton", enableCooldown);
        currentState = AnimStates.CREATE_COLUMNS;
        return 0;
    }

    IEnumerator FadeInEquiv()
    {
        while (equivCanvasGroup.alpha < 1f)
        {
            equivCanvasGroup.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }
    }

    IEnumerator FadeOutEquiv()
    {
        while (equivCanvasGroup.alpha > 0.1f)
        {
            equivCanvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
        equivCanvasGroup.alpha = 0;
    }
    #endregion

    #region CreatingTheTable
    public int CreateColumns()
    {
        StartCoroutine(buildColumns());
        currentState = AnimStates.CREATE_ROWS;
        return 0;
    }

    public int CreateRows()
    {
        StartCoroutine(buildRows());
        currentState = AnimStates.FILL_FIRST_COLUMN;
        return 0;
    }

    IEnumerator buildColumns()
    {
        description.text = "Primero hagamos una lista desde <#" + green + ">0</color> hasta <#" + green + ">" + sum.ToString() + "</color> árboles.";
        
        currentX = xMin + 1.5f;
        currentZ = zMin + 1.5f;

        for (int i = 0; i <= sum; i++)
        {
            GameObject number = Instantiate(treeNumberPrefab);
            Vector3 dest = new Vector3(currentX, minBounds.position.y, currentZ);
            number.GetComponent<EditableText>().setText(i.ToString());
            number.transform.position = RandomPos();
            number.SetActive(true);
            yield return StartCoroutine(MoveObject(number.transform, dest, 1f));
            currentX += xStep;
        }
        EnableButton();
    }

    IEnumerator buildRows()
    {
        description.text = "Ahora listemos los elementos de nuestro arreglo como filas, <#" + green + ">nótese que el primer elemento es 0 pues consideramos tanto el caso donde no tenemos números en el arreglo, así como el caso de no tener árboles para crecer.</color>";
        
        currentX = xMin - 1.5f;
        currentZ = zMin - 1.5f;

        for (int i = 0; i <= length; i++)
        {
            GameObject number = Instantiate(waterCubeNumPrefab);
            Vector3 dest = new Vector3(currentX, minBounds.position.y, currentZ);
            string txt = (i == 0) ? "0" : GlobalValues.instance.A[i - 1].ToString();
            number.GetComponent<EditableText>().setText(txt);
            number.transform.position = RandomPos();
            number.SetActive(true);
            yield return StartCoroutine(MoveObject(number.transform, dest, 1f));
            currentZ -= zStep;
        }
        EnableButton();
    }
    #endregion

    #region FillingTheTable
    public int Fill1stColumn()
    {
        StartCoroutine(fillFirstColumn());
        currentState = AnimStates.FILL_FIRST_ROW;
        return 0;
    }

    public int Fill1stRow()
    {
        StartCoroutine(fillFirstRow());
        currentState = AnimStates.FILL_AT;
        _i = 1;
        _j = 1;
        return 0;
    }

    IEnumerator fillFirstColumn()
    {
        description.text = "Sabemos que si no necesitamos crear un árbol (0), entonces basta con un conjunto vacío para satisfacer la demanda de 0 árboles.";
        
        currentX = xMin + 1.5f;
        currentZ = zMin - 1.5f;

        Vector3 columnTarget = new Vector3(currentX, columnIndicator.position.y, columnIndicator.position.z);
        Vector3 rowTarget = new Vector3(rowIndicator.position.x, rowIndicator.position.y, currentZ);

        yield return StartCoroutine(MoveObject(columnIndicator, columnTarget, 0.5f));

        for (int i = 0; i <= length; i++)
        {
            DP[i, 0] = true;
            yield return StartCoroutine(MoveObject(rowIndicator, rowTarget, 0.5f));
            GameObject bObj = boolObj(DP[i, 0]);
            Vector3 dest = new Vector3(currentX, 0f, currentZ);
            bObj.transform.position = dest;
            bObj.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            currentZ -= zStep;
            rowTarget.z = currentZ;
        }

        rowIndicator.position = rowInitalPos;
        columnIndicator.position = columnInitialPos;
        EnableButton();
    }

    IEnumerator fillFirstRow()
    {
        description.text = "Pero si no tenemos ningún cubo de agua, entonces no podremos hacer crecer a ningún árbol y solo tendremos un tronco marchito :c";
        
        currentX = xMin + 1.5f + xStep;
        currentZ = zMin - 1.5f;

        Vector3 columnTarget = new Vector3(currentX, columnIndicator.position.y, columnIndicator.position.z);
        Vector3 rowTarget = new Vector3(rowIndicator.position.x, rowIndicator.position.y, currentZ);

        yield return StartCoroutine(MoveObject(rowIndicator, rowTarget, 0.5f));

        for (int i = 1; i <= sum; i++)
        {
            DP[0, i] = false;
            yield return StartCoroutine(MoveObject(columnIndicator, columnTarget, 0.5f));
            GameObject bObj = boolObj(DP[0, i]);
            Vector3 dest = new Vector3(currentX, 0f, currentZ);
            bObj.transform.position = dest;
            bObj.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            currentX += xStep;
            columnTarget.x = currentX;
        }

        rowIndicator.position = rowInitalPos;
        columnIndicator.position = columnInitialPos;
        currentX = xMin + 1.5f + xStep;
        currentZ = zMin - 1.5f - zStep;
        EnableButton();
    }

    int FillAt()
    {
        if (_j == sum + 1 && _i == length)
        {
            EndAnim();
            return -1;
        }
        if (_j > sum && _i < length)
        {
            _j = 1;
            _i++;
            currentX = xMin + 1.5f + xStep;
            currentZ -= zStep;
        }
        StartCoroutine(fillCellA());
        return 0;
    }

    int FillComp()
    {
        StartCoroutine(fillCellB());
        return 0;
    }

    int FillResult()
    {
        StartCoroutine(fillCellC());
        return 0;
    }

    void EndAnim()
    {
        currentState = AnimStates.EXPLANATION_0;
        endPanel.SetActive(true);
        description.text = "";
        endText.text = DP[length, sum] ? "Se encontró un subconjunto con una suma de " + sum.ToString() : "No se encontró un subconjunto con una suma de " + sum.ToString();
    }

    IEnumerator fillCellA()
    {
        Vector3 columnTarget = new Vector3(currentX, columnIndicator.position.y, columnIndicator.position.z);
        Vector3 rowTarget = new Vector3(rowIndicator.position.x, rowIndicator.position.y, currentZ);
        yield return StartCoroutine(MoveObject(columnIndicator, columnTarget, 0.5f));
        yield return StartCoroutine(MoveObject(rowIndicator, rowTarget, 0.5f));
        description.text = "¿Tenemos más árboles (<#" + green + ">" + _j.ToString() + "</color>) que cubos de agua (<#" + green + ">" + GlobalValues.instance.A[_i - 1].ToString() + "</color>)?";
        currentState = AnimStates.FILL_AT_COMP;
        Invoke("EnableButton", 1f);
        //EnableButton();
    }

    IEnumerator fillCellB()
    {
        Vector3 columnTarget = new Vector3(currentX, columnIndicator.position.y, columnIndicator.position.z);
        Vector3 rowTarget = new Vector3(rowIndicator.position.x, rowIndicator.position.y, currentZ);

        if (_j < GlobalValues.instance.A[_i - 1])
        {
            DP[_i, _j] = DP[_i - 1, _j];
            rowTarget.z += zStep;
            description.text = "<color=red>No:</color> Como tenemos menos árboles que cubos, entonces solo tomamos el resultado de la casilla de arriba.";
            yield return StartCoroutine(MoveObject(rowIndicator, rowTarget, 0.5f));
            GameObject bObj = boolObj(DP[_i, _j]);
            rowTarget.z -= zStep;
            yield return StartCoroutine(MoveObject(rowIndicator, rowTarget, 0.5f));
            Vector3 dest = new Vector3(currentX, minBounds.position.y, currentZ);
            bObj.transform.position = dest;
            bObj.SetActive(true);
        }
        else
        {
            DP[_i, _j] = DP[_i - 1, _j] || DP[_i - 1, _j - GlobalValues.instance.A[_i - 1]];
            rowTarget.z += zStep;
            description.text = "<#" + green + ">Sí:</color> Tenemos dos opciones, en primer lugar podríamos solo tomamos el resultado de la casilla de arriba.\n" +
                "Pero como a " + _j.ToString() + " árboles podemos abastecerles de " + GlobalValues.instance.A[_i - 1].ToString() + " cubos de agua, entonces tabién podríamos preguntarnos: ¿Tenemos ya un subjonjunto que satisfaga la demanda de los árboles faltantes, es decir <#" + green + ">" + _j.ToString() + "-" + GlobalValues.instance.A[_i - 1].ToString() + "=" + (_j - GlobalValues.instance.A[_i - 1]).ToString() + "</color>?";
            yield return StartCoroutine(MoveObject(rowIndicator, rowTarget, 1f));
            rowTarget.z -= zStep;
            yield return StartCoroutine(MoveObject(rowIndicator, rowTarget, 1f));
            columnTarget.x = xMin + 1.5f + (_j - GlobalValues.instance.A[_i - 1]) * xStep;
            yield return StartCoroutine(MoveObject(columnIndicator, columnTarget, 1f));
            rowTarget.z += zStep;
            yield return StartCoroutine(MoveObject(rowIndicator, rowTarget, 1f));
            columnTarget.x = currentX;
            yield return StartCoroutine(MoveObject(columnIndicator, columnTarget, 1f));
            rowTarget.z -= zStep;
            yield return StartCoroutine(MoveObject(rowIndicator, rowTarget, 1f));
            GameObject bObj = boolObj(DP[_i, _j]);
            Vector3 dest = new Vector3(currentX, minBounds.position.y, currentZ);
            bObj.transform.position = dest;
            bObj.SetActive(true);
        }
        currentX += xStep;
        currentState = AnimStates.FILL_AT_RESULT;
        Invoke("EnableButton", 1f);
        //EnableButton();
    }

    IEnumerator fillCellC()
    {
        if (_j < GlobalValues.instance.A[_i - 1])
        {
            description.text = DP[_i, _j] ? "Ya teníamos un subconjunto que nos diera <#" + green + ">" + _j.ToString() + "</color> de suma, por lo tanto podemos hacer crecer los árboles" : "Aún no tenemos un subconjunto cuya suma pueda abastecer a <color=red>" + _j.ToString() + " árboles</color>, por lo tanto se marchitan.";
        }
        else
        {
            description.text = DP[_i, _j] ? "Por lo menos una condición se cumplió, por lo tanto podemos hacer crecer los <#" + green + ">" + _j.ToString() + " árboles.</color>" : "Ninguna condición se cumplió, aún no tenemos un subconjunto cuya suma pueda abastecer a <color=red>" + _j.ToString() + " árboles</color>, por lo tanto se marchitan.";
        }
        _j++;
        yield return new WaitForSeconds(2f);
        currentState = AnimStates.FILL_AT;
        EnableButton();
    }
    #endregion

    void SetSumAndLength()
    {
        sum = GlobalValues.instance.sum;
        length = GlobalValues.instance.A.Count;
    }

    void CalculateSteps()
    {
        xMin = minBounds.position.x;
        xMax = maxBounds.position.x;
        zMin = minBounds.position.z;
        zMax = maxBounds.position.z;

        xStep = Mathf.Abs(xMax - xMin + 0.5f) / GlobalValues.instance.sum;
        zStep = Mathf.Abs(zMin - 0.5f - zMax) / GlobalValues.instance.A.Count;
    }

    Vector3 RandomPos()
    {
        return spawnPos[UnityEngine.Random.Range(0, spawnPos.Length - 1)].position;
    }

    GameObject boolObj(bool b)
    {
        GameObject obj;
        if (b)
            obj = Instantiate(treePrefab);
        else
            obj = Instantiate(rootPrefab);
        return obj;
    }

    IEnumerator MoveObject(Transform objToMove, Vector3 dest, float timeToReach)
    {
        float speed = Vector3.Distance(objToMove.position, dest) / timeToReach;
        Vector3 dir = (dest - objToMove.position).normalized;
        float t = 0;
        while (t < timeToReach)
        {
            objToMove.Translate(dir * speed * Time.deltaTime);
            t += Time.deltaTime;
            yield return null;
        }
    }
}
