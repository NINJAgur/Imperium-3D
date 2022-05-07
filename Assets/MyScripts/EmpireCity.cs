using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmpireCity : MonoBehaviour
{
    public HexCell city;

    public float capitalRotationSpeed = 30f;
    public float capitalBobSpeed = 10f;

    void Update()
    {
        transform.Rotate(Vector3.up, capitalRotationSpeed * Time.deltaTime, Space.World);
        transform.position = transform.position + new Vector3(0f, 0.01f * Mathf.Sin(Time.time * capitalBobSpeed), 0f);
    }
}
