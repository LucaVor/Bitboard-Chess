using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Predict : MonoBehaviour
{
    public Vector3 origin = Vector3.zero;
    public Vector3 target = Vector3.forward * 6 + Vector3.right * 2;
    public float gravity = 4;
    public float time = 1.5f;
    public float simTime = 0;
    public float simulationSpeed = 0.5f;

    public bool StartSimulation = false;
    public bool simulating = false;

    public Vector3 velocity;
    public float current_gravity = 0;

    void Update()
    {
        if (StartSimulation)
        {
            StartSimulation = false;
            simulating = true;
            float xf = target.x;
            float yf = target.y + (gravity * (time * time));
            float zf = target.z;

            float vx = (xf - origin.x) / time;
            float vy = (yf - origin.y) / time;
            float vz = (zf - origin.z) / time;

            velocity = new Vector3(vx, vy, vz);
        }

        if (simulating)
        {
            origin += velocity * Time.deltaTime * simulationSpeed;
            origin += Vector3.up * -current_gravity;
            current_gravity += gravity * Time.deltaTime * simulationSpeed;
        }
    }

    void OnDrawGizmos()
    {
        float xf = target.x;
        float yf = target.y - 0.5f * -gravity * (time * time);
        float zf = target.z;

        float vx = (xf - origin.x) / time;
        float vy = (yf - origin.y) / time;
        float vz = (zf - origin.z) / time;

        Vector3 v = new Vector3 (vx, vy, vx);

        float oxf = origin.x + vx * simTime;
        float oyf = (origin.y + vy * simTime) - 0.5f * gravity * (simTime * simTime);
        float ozf = origin.z + vz * simTime;

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere (new Vector3(oxf, oyf, ozf), 0.5f); 

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere (origin, 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere (target, 0.5f);

        Gizmos.color = Color.green;
        Gizmos.DrawLine (origin, origin + v);
    }
}
