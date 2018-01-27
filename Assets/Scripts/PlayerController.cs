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

    // UI
    public Text m_GameOverText;

    void Awake () {
        m_Camera = FindObjectOfType<Camera>();
        m_Rigidbody = GetComponent<Rigidbody2D>();
	}

    private void Start() {
        m_Arrow.gameObject.SetActive(false);
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
            m_Arrow.gameObject.SetActive(false);
        }
        else {
            m_Arrow.transform.rotation = Quaternion.LookRotation(Vector3.forward, m_FollowTarget.position - transform.position);
            m_HitWall = false;
            m_HitEnergySphere = false;

            if (Input.GetMouseButton(0)) {
                m_Arrow.gameObject.SetActive(true);
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
        //StartCoroutine(MoveToNothing(target.position));
        var newPosition = transform.position + (target.position - transform.position).normalized * m_JumpDistance;
        newPosition.z = 0;
        transform.position = newPosition;
        GameOver();
        m_IsMoving = false;
        // TODO: lose
    }

    private void HitWall() {
        m_IsMoving = true;
        // TODO: max distance, don't enter wall
        //float wallDistance = (transform.position - m_WallHitPoint).magnitude;
        transform.position = m_WallHitPoint;
        GameOver();
        m_IsMoving = false;
        //StartCoroutine(MoveToWall(target.transform.position));
        // TODO: lose
    }

    private void HitEnergySphere() {
        var target = m_JumpTarget;
        m_IsMoving = true;
        var controller = target.GetComponent<EnergySphereController>();
        controller.DisableCollider();
        var newPosition = target.transform.position;
        newPosition.z = 0;
        transform.position = newPosition;
        m_IsMoving = false;
        //StartCoroutine(MoveToEnergySphere(target.transform.position));
    }

    private IEnumerator MovePlayer(Vector3 newPosition) {
        LeanTween.move(gameObject, newPosition, .5f);
        yield return new WaitForSeconds(.5f);
    }

    /*
    private IEnumerator MoveToNothing(Vector3 newPosition) {
        yield return MovePlayer(newPosition);
        m_IsMoving = false;
    }

    private IEnumerator MoveToWall(Vector3 newPosition) {
        yield return MovePlayer(newPosition);
        m_IsMoving = false;
    }

    private IEnumerator MoveToEnergySphere(Vector3 newPosition) {
        yield return MovePlayer(newPosition);
        m_IsMoving = false;
    }*/
}
