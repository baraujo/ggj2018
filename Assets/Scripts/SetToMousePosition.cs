using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetToMousePosition : MonoBehaviour {

    private Camera m_Camera;

    private void Start() {
        m_Camera = FindObjectOfType<Camera>();
    }

    void Update () {
        transform.position = m_Camera.ScreenToWorldPoint(Input.mousePosition);
    }
}
