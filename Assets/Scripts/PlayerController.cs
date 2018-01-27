using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

    public ArrowController m_Arrow;
    public Transform m_FollowTarget;
    public CinemachineVirtualCamera m_VirtualCamera;
    private Camera m_Camera;
    private Rigidbody2D m_Rigidbody;
    private int m_LayerMask;
    private int m_JumpDistance = 18;
    
    // Game logic
    public bool m_HitEnergySphere, m_HitWall;
    public bool m_IsMoving, m_GameOver;
    private GameObject m_JumpTarget;
    private Vector3 m_WallHitPoint;

    // Animations
    public GameObject m_TeleportAnimation;
    public SpriteRenderer m_renderer;

    // UI
    public Text m_GameOverText;

    void Awake () {
        m_Camera = FindObjectOfType<Camera>();
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_renderer = GetComponent<SpriteRenderer>();
	}

    private void Start() {
        m_Arrow.m_arrowObject.SetActive(false);
        m_LayerMask = 1 << LayerMask.NameToLayer("Walls") | 1 << LayerMask.NameToLayer("EnergySphere");
        m_IsMoving = false;
        m_GameOver = false;
    }

    void Update() {
        if (m_IsMoving) return;
        if (m_GameOver) {
            if (Input.GetMouseButtonDown(0)) {
                SceneManager.LoadScene(0);
            }
            return;
        }

        if (Input.GetMouseButtonUp(0)) {
            if (m_HitEnergySphere) {
                HitEnergySphere();
            }
            else if (m_HitWall) {
                HitWall();
            }
            else {
                HitNothing();
            }
            m_Arrow.m_renderer.color = Color.white;
            m_Arrow.m_arrowObject.SetActive(false);
        }
        else {
            m_Arrow.transform.rotation = Quaternion.LookRotation(Vector3.forward, m_FollowTarget.position - transform.position);
            m_HitWall = false;
            m_HitEnergySphere = false;

            if (Input.GetMouseButton(0)) {
                m_Arrow.m_arrowObject.SetActive(true);
                RaycastHit2D hit = Physics2D.Raycast(m_Arrow.transform.position, m_FollowTarget.position - transform.position, m_JumpDistance, m_LayerMask);
                if (hit.collider != null) {
                    if (hit.collider.CompareTag("Wall")) {
                        Debug.DrawLine(m_Arrow.transform.position, hit.point, Color.red, m_JumpDistance);
                        m_Arrow.m_renderer.color = Color.red;
                        m_JumpTarget = hit.collider.gameObject;
                        m_WallHitPoint = hit.point;
                        m_HitWall = true;
                    }
                    else if (hit.collider.CompareTag("EnergySphere")) {
                        Debug.DrawLine(m_Arrow.transform.position, hit.point, Color.blue, m_JumpDistance);
                        m_Arrow.m_renderer.color = Color.blue;
                        m_JumpTarget = hit.collider.gameObject;
                        m_HitEnergySphere = true;
                    }
                }
                else {
                    m_JumpTarget = m_FollowTarget.gameObject;
                    m_Arrow.m_renderer.color = Color.white;
                }
            }
        }
    }

    private void GameOver() {
        m_VirtualCamera.Follow = null;
        m_Rigidbody.bodyType = RigidbodyType2D.Dynamic;
        m_Rigidbody.velocity = Vector3.zero;
        m_Rigidbody.gravityScale = 3;
        m_Rigidbody.AddTorque(50);
        m_GameOverText.enabled = true;
        m_GameOver = true;
    }

    private void HitNothing() {
        m_IsMoving = true;
        var target = m_FollowTarget;
        var newPosition = transform.position + (target.position - transform.position).normalized * m_JumpDistance;
        newPosition.z = 0;
        transform.position = newPosition;
        GameOver();
        m_IsMoving = false;
        // TODO: lose
    }

    private void HitWall() {
        m_IsMoving = true;
        transform.position = m_WallHitPoint;
        GameOver();
        m_IsMoving = false;
        // TODO: lose
    }

    private void HitEnergySphere() {
        var target = m_JumpTarget;
        m_IsMoving = true;
        var controller = target.GetComponent<EnergySphereController>();
        controller.DisableCollider();
        var newPosition = target.transform.position;
        StartCoroutine(MoveToEnergySphere(target.transform.position));
    }

    private IEnumerator MoveToEnergySphere(Vector3 position) {
        m_renderer.enabled = false;
        position.z = 0; 
        var transformPosition = (transform.position + position) / 2;
        var stretchSize = (position - transform.position).magnitude;
        transform.position = position;
        m_TeleportAnimation.SetActive(true);
        m_TeleportAnimation.transform.position = transformPosition;
        m_TeleportAnimation.transform.localScale = new Vector3(1, stretchSize, 1);
        yield return new WaitForSeconds(0.125f);
        m_TeleportAnimation.SetActive(false);
        m_renderer.enabled = true;
        m_IsMoving = false;
    }
}
