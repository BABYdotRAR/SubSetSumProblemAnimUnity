using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalValues : MonoBehaviour
{
    public static GlobalValues instance;
    public List<int> A;
    public int sum;

    public void AddNumber(int num) => A.Add(num);

    public void RemoveNumber(int num) => A.Remove(num);

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }
}
