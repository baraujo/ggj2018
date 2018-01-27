using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowController : MonoBehaviour {

    public GameObject m_arrowObject;
    public SpriteRenderer m_renderer;

    private void Awake() {
        m_renderer = m_arrowObject.GetComponent<SpriteRenderer>();
    }
}
