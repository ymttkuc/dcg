using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogView : MonoBehaviour {

    public List<string> log = new List<string>();
    
    private void Awake() {

    }

    // Use this for initialization
    void Start () {
        
		
	}
	
	// Update is called once per frame
	void Update () {

    }

    public void ReroadLog(List<string> s) {
        log = s;
        var t = "";
        for (int i = 0; i < log.Count; ++i) {
            t += log[i] + "\n";
        }
        GetComponent<Text>().text = t;
    }
}
