using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleFollow : MonoBehaviour
{
    Vector3 diff;

    public GameObject target;
    public float followSpeed;

    private void Start()
    {
        diff = target.transform.position - transform.position;
    }

    private void LateUpdate()
    {
        transform.position = target.transform.position - diff;
    }
}
