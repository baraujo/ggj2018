using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergySphereController : MonoBehaviour {

    public bool m_FinalSphere;
    private Collider2D m_Collider;

    private void Awake() {
        m_Collider = GetComponent<BoxCollider2D>();
    }

    void Start () {
		
	}
	
	void Update () {
		
	}

    public void DisableCollider() {
        m_Collider.enabled = false;
    }
}
