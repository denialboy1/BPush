using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallCollider : MonoBehaviour {
    public GameObject Parent;
    public float r;
	// Use this for initialization
	void Start () {
        Parent = gameObject.transform.parent.parent.gameObject;
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = Parent.transform.position + new Vector3(0, r, 1.51f + r);  
    }
    public void get(float r)
    {
        this.r = r;
    }
}
