using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public unsafe class ListTesting : MonoBehaviour
{
    public List<int> testing = new List<int>(3);

    void Start()
    {
        testing.Add(0);
//        int* pointerToListValue = testing[0];
        testing.Add(1);
        testing.Add(2);
    }
}