using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteNumber : MonoBehaviour
{
    int num;

    public void SetNum(int n) => num = n;

    public void Delete()
    {
        GlobalValues.instance.RemoveNumber(num);
        Destroy(gameObject);
    }
}
