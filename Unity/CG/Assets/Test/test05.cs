using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//handにあたる
public class test05 : MonoBehaviour {

    public GameObject obj_origin;
    public GameObject obj_copy;

	// Use this for initialization
	void Start () {
        obj_copy = Instantiate(obj_origin, obj_origin.transform.position,
            obj_origin.transform.rotation, obj_origin.transform);
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(1)) {
            if (obj_copy) {
                obj_copy = Instantiate(obj_origin, transform.position,
                    transform.rotation, transform);
            }
        }
	}
}
