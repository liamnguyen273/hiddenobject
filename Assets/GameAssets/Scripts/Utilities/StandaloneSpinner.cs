using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandaloneSpinner : MonoBehaviour
{
    [SerializeField] private float _speed = 10f;

    private void OnEnable()
    {
        transform.rotation = Quaternion.identity;
    }

    private void Update()
    {
        var dt = Time.deltaTime;

        var r = transform.rotation.eulerAngles;
        r.z += dt * _speed;
        transform.rotation = Quaternion.Euler(r);
    }
}
