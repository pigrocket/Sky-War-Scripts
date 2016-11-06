using UnityEngine;
using System.Collections;

public class sweepcast : MonoBehaviour {
    public float collisionCheckDistance;
    public bool aboutToCollide;
    public float distanceToCollision;
    public Rigidbody rb;

    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    void Update() {
        RaycastHit hit;
        if (rb.SweepTest (transform.Find("MainCamera").transform.forward, out hit, collisionCheckDistance)) {
            aboutToCollide = true;
            distanceToCollision = hit.distance;
        }
		else aboutToCollide = false;
    }
}
