using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//cardにあたる
public class test06 : MonoBehaviour {

    public GameObject obj_origin;
    public GameObject container;

	// Use this for initialization
	void Start () {
        if (!container) {
            Debug.Log("ins");
            container = Instantiate(obj_origin,
                obj_origin.transform.position,
                obj_origin.transform.rotation, obj_origin.transform);
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (container) {
            container.GetComponent<Number>();
            Debug.Log("clear");
        }
	}
}
