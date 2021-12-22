using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CreateTable : MonoBehaviour
{
    public Transform minBounds;
    public Transform maxBounds;
    public GameObject numberPrefab;
    public GameObject boolPrefab;
    public float numberSpeed;
    public float boolSpeed;
    public Transform[] spawnPos;
    float xStep, zStep;

    /*
    private void Start()
    {
        GlobalValues.instance.AddNumber(2);
        GlobalValues.instance.AddNumber(8);
        GlobalValues.instance.AddNumber(7);
        GlobalValues.instance.AddNumber(4);
        GlobalValues.instance.sum = 6;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Create();
    }
    */
    public void Create()
    {
        CalculateSteps();
        StartCoroutine(NumAnim());
    }

    void CalculateSteps()
    {
        float xMin = minBounds.position.x + 0.5f;
        float xMax = maxBounds.position.x;
        float zMin = minBounds.position.z - 0.5f;
        float zMax = maxBounds.position.z;

        xStep = Mathf.Abs(xMax - xMin) / GlobalValues.instance.sum;
        zStep = Mathf.Abs(zMin - zMax) / GlobalValues.instance.A.Count;
    }

    IEnumerator NumAnim()
    {
        float currentX = minBounds.position.x + 1.5f;
        float currentZ = minBounds.position.z + 1.5f;
        for (int i = 0; i <= GlobalValues.instance.sum; i++)
        {
            GameObject num = Instantiate(numberPrefab);
            Vector3 dest = new Vector3(currentX, minBounds.position.y, currentZ);
            num.GetComponentInChildren<TextMeshPro>().text = i.ToString();
            num.transform.position = spawnPos[Random.Range(0, spawnPos.Length - 1)].position;
            num.SetActive(true);
            while(Vector3.Distance(num.transform.position, dest) > 0.25f)
            {
                Vector3 direction = (dest - num.transform.position).normalized;
                num.transform.Translate(direction * numberSpeed * Time.deltaTime, Space.World);
                yield return null;
            }
            currentX += xStep;
        }
        currentX = minBounds.position.x - 1.5f;
        currentZ = minBounds.position.z - 1.5f;
        for (int i = 0; i <= GlobalValues.instance.A.Count; i++)
        {
            GameObject num = Instantiate(numberPrefab);
            Vector3 dest = new Vector3(currentX, minBounds.position.y, currentZ);
            num.GetComponentInChildren<TextMeshPro>().text = (i == 0) ? "0" : GlobalValues.instance.A[i - 1].ToString();
            num.transform.position = spawnPos[Random.Range(0, spawnPos.Length - 1)].position;
            num.SetActive(true);
            while (Vector3.Distance(num.transform.position, dest) > 0.25f)
            {
                Vector3 direction = (dest - num.transform.position).normalized;
                num.transform.Translate(direction * numberSpeed * Time.deltaTime, Space.World);
                yield return null;
            }
            currentZ -= zStep;
        }

        StartCoroutine(booleansAnim());
    }

    IEnumerator booleansAnim()
    {
        bool[,] DP = new bool[GlobalValues.instance.A.Count + 1,GlobalValues.instance.sum + 1];
        float currentX = minBounds.position.x + 1.5f;
        float currentZ = minBounds.position.z - 1.5f;

        for (int i = 0; i <= GlobalValues.instance.A.Count; i++)
        {
            DP[i, 0] = true;
            GameObject bObj = boolObj(DP[i, 0]);
            Vector3 dest = new Vector3(currentX, minBounds.position.y, currentZ);
            bObj.transform.position = spawnPos[Random.Range(0, spawnPos.Length - 1)].position;
            bObj.SetActive(true);
            while (Vector3.Distance(bObj.transform.position, dest) > Random.Range(0.25f, 0.5f))
            {
                Vector3 direction = (dest - bObj.transform.position).normalized;
                bObj.transform.Translate(direction * boolSpeed * Time.deltaTime, Space.World);
                yield return null;
            }
            currentZ -= zStep;
        }

        currentZ = minBounds.position.z - 1.5f;
        currentX += xStep;

        for (int i = 1; i <= GlobalValues.instance.sum; i++)
        {
            DP[0, i] = false;
            GameObject bObj = boolObj(DP[0, i]);
            Vector3 dest = new Vector3(currentX, minBounds.position.y, currentZ);
            bObj.transform.position = spawnPos[Random.Range(0, spawnPos.Length - 1)].position;
            bObj.SetActive(true);
            while (Vector3.Distance(bObj.transform.position, dest) > Random.Range(0.25f, 0.5f))
            {
                Vector3 direction = (dest - bObj.transform.position).normalized;
                bObj.transform.Translate(direction * boolSpeed * Time.deltaTime, Space.World);
                yield return null;
            }
            currentX += xStep;
        }

        currentX = minBounds.position.x + 1.5f + xStep;
        currentZ = minBounds.position.z - 1.5f - zStep;
        for (int i = 1; i <= GlobalValues.instance.A.Count; i++)
        {
            for (int j = 1; j <= GlobalValues.instance.sum; j++)
            {
                if(j < GlobalValues.instance.A[i - 1])
                {
                    DP[i, j] = DP[i - 1, j];
                    GameObject bObj = boolObj(DP[i, j]);
                    Vector3 dest = new Vector3(currentX, minBounds.position.y, currentZ);
                    bObj.transform.position = spawnPos[Random.Range(0, spawnPos.Length - 1)].position;
                    bObj.SetActive(true);
                    while (Vector3.Distance(bObj.transform.position, dest) > Random.Range(0.25f, 0.5f))
                    {
                        Vector3 direction = (dest - bObj.transform.position).normalized;
                        bObj.transform.Translate(direction * boolSpeed * Time.deltaTime, Space.World);
                        yield return null;
                    }
                    currentX += xStep;
                }
                if(j >= GlobalValues.instance.A[i - 1])
                {
                    DP[i, j] = DP[i - 1, j] || DP[i - 1, j - GlobalValues.instance.A[i - 1]];
                    GameObject bObj = boolObj(DP[i, j]);
                    Vector3 dest = new Vector3(currentX, minBounds.position.y, currentZ);
                    bObj.transform.position = spawnPos[Random.Range(0, spawnPos.Length - 1)].position;
                    bObj.SetActive(true);
                    while (Vector3.Distance(bObj.transform.position, dest) > Random.Range(0.25f, 0.5f))
                    {
                        Vector3 direction = (dest - bObj.transform.position).normalized;
                        bObj.transform.Translate(direction * boolSpeed * Time.deltaTime, Space.World);
                        yield return null;
                    }
                    currentX += xStep;
                }
            }
            currentX = minBounds.position.x + 1.5f + xStep;
            currentZ -= zStep;
        }
    }

    GameObject boolObj(bool b)
    {
        GameObject obj = Instantiate(boolPrefab);
        if (b)
            obj.GetComponent<ChangeColor>().Green();
        else
            obj.GetComponent<ChangeColor>().Red();
        return obj;
    }
}
