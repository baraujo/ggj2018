using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class PlayerController : MonoBehaviour {

    public ArrowController m_Arrow;
    public Transform m_FollowTarget;
    public CinemachineVirtualCamera m_VirtualCamera;
    public GameObject intro;
    private Camera m_Camera;
    private Rigidbody2D m_Rigidbody;
    private int m_LayerMask;
    private int m_JumpDistance = 18;
    private Stopwatch m_Stopwatch;
    
    // Animations
    public GameObject m_TeleportAnimation;
    public SpriteRenderer m_renderer;

    // UI
    [Header("UI")]
    public Text m_GameOverText;
    public Text m_YouWonText;
    public Text m_GameTime;

    // Game state
    [Header("Game state")]
    public bool m_HitEnergySphere;
    public bool m_HitWall;
    public bool m_IsMoving, m_GameOver;
    public bool m_GameStarted;
    public float m_MaxTimeOnSpheres;
    private GameObject m_JumpTarget;
    private Vector3 m_WallHitPoint;

    // Audio
    [Header("Audio")]
    public AudioClip bgMusic;
    public AudioClip dashFX;
    public AudioClip buttonClickFX;
    public AudioClip winFX;
    public AudioClip loseFX;

    AudioSource audioSource;

    void Awake () {
        m_Camera = FindObjectOfType<Camera>();
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_renderer = GetComponent<SpriteRenderer>();
        m_Stopwatch = new Stopwatch();
        audioSource = GetComponent<AudioSource>();
	}

    private void Start() {
        m_Arrow.m_arrowObject.SetActive(false);
        m_LayerMask = 1 << LayerMask.NameToLayer("Walls") | 1 << LayerMask.NameToLayer("EnergySphere");
        m_IsMoving = false;
        m_GameOver = false;
        m_GameStarted = false;
    }

    public void StartGame() {
        Destroy(intro);
        m_Stopwatch.Reset();
        m_Stopwatch.Start();
        StartCoroutine(DelayGameStart());
    }

    private IEnumerator DelayGameStart() {
        yield return new WaitForSeconds(0.25f);
        audioSource.PlayOneShot(bgMusic);
        m_GameStarted = true;
    }

    void Update() {
        if (!m_GameStarted) return;

        if (m_GameOver) {
            if (Input.GetMouseButtonDown(0) && m_GameStarted) {
                SceneManager.LoadScene(0);
            }
            return;
        }
        m_GameTime.text = string.Format("{0:00}:{1:00}:{2:000}", m_Stopwatch.Elapsed.Minutes, m_Stopwatch.Elapsed.Seconds, m_Stopwatch.Elapsed.Milliseconds);

        if (m_IsMoving) return;
        if (Input.GetMouseButtonUp(0) && m_GameStarted) {
            if (m_HitEnergySphere) {
                HitEnergySphere();
            }
            else if (m_HitWall) {
                StartCoroutine(HitWall());
            }
            else {
                StartCoroutine(HitNothing());
            }
            m_Arrow.m_renderer.color = Color.white;
            m_Arrow.m_arrowObject.SetActive(false);
        }
        else {
            m_Arrow.transform.rotation = Quaternion.LookRotation(Vector3.forward, m_FollowTarget.position - transform.position);
            m_HitWall = false;
            m_HitEnergySphere = false;

            if (Input.GetMouseButtonDown(0)) {
                audioSource.PlayOneShot(buttonClickFX);
            }
            
            if (Input.GetMouseButton(0)) {
                m_Arrow.m_arrowObject.SetActive(true);
                RaycastHit2D hit = Physics2D.Raycast(m_Arrow.transform.position, m_FollowTarget.position - transform.position, m_JumpDistance, m_LayerMask);
                if (hit.collider != null) {
                    if (hit.collider.CompareTag("Wall")) {
                        Debug.DrawLine(m_Arrow.transform.position, hit.point, Color.red, m_JumpDistance);
                        m_JumpTarget = hit.collider.gameObject;
                        m_WallHitPoint = hit.point;
                        m_HitWall = true;
                    }
                    else if (hit.collider.CompareTag("EnergySphere")) {
                        Debug.DrawLine(m_Arrow.transform.position, hit.point, Color.blue, m_JumpDistance);
                        m_JumpTarget = hit.collider.gameObject;
                        m_HitEnergySphere = true;
                    }
                }
                else {
                    m_JumpTarget = m_FollowTarget.gameObject;
                }
            }
        }
    }

    private void GameOver() {
        audioSource.PlayOneShot(loseFX, 0.35f);
        m_VirtualCamera.Follow = null;
        m_Rigidbody.bodyType = RigidbodyType2D.Dynamic;
        m_Rigidbody.velocity = Vector3.zero;
        m_Rigidbody.gravityScale = 3;
        m_Rigidbody.AddTorque(50);
        m_GameOver = true;
        m_GameOverText.enabled = true;
        m_GameOverText.text = "No more energy!\nClick mouse to try again!";
    }

    private IEnumerator HitNothing() {
        m_IsMoving = true;
        var target = m_FollowTarget;
        var newPosition = transform.position + (target.position - transform.position).normalized * m_JumpDistance;
        yield return Teleport(newPosition);
        GameOver();
    }

    private IEnumerator HitWall() {
        m_IsMoving = true;
        yield return Teleport(m_WallHitPoint);
        GameOver();
    }

    private void HitEnergySphere() {
        var target = m_JumpTarget;
        m_IsMoving = true;
        var controller = target.GetComponent<EnergySphereController>();
        controller.DisableCollider();
        var newPosition = target.transform.position;
        StartCoroutine(MoveToEnergySphere(controller));
    }

    private IEnumerator MoveToEnergySphere(EnergySphereController controller) {
        yield return Teleport(controller.transform.position);
        if (controller.m_FinalSphere == true) {
            YouWon();
        }
        else {
            // TODO: start sphere animation
            m_IsMoving = false;
            StartCoroutine(EnergySphereFallTimer());
        }
    }

    private IEnumerator EnergySphereFallTimer() {
        float elapsed = 0;
        bool timeout = false;
        while (!m_IsMoving && !timeout) {
            elapsed += Time.deltaTime;
            if(elapsed > m_MaxTimeOnSpheres) {
                //TODO: animation for time over
                Timeout();
                timeout = true;
            }
            yield return null;
        }
    }

    private void Timeout() {
        m_VirtualCamera.Follow = null;
        m_GameOver = true;
        m_GameOverText.enabled = true;
        m_GameOverText.text = "Energy overload!\nClick mouse to try again!";
    }

    private void YouWon() {
        m_YouWonText.enabled = true;
        m_GameOver = true;
        audioSource.PlayOneShot(winFX);
    }

    private IEnumerator Teleport(Vector3 position) {
        audioSource.PlayOneShot(dashFX);
        position.z = 0;
        m_renderer.enabled = false;
        var transformPosition = (transform.position + position) / 2;
        var stretchSize = (position - transform.position).magnitude;
        transform.position = position;
        m_TeleportAnimation.SetActive(true);
        m_TeleportAnimation.transform.position = transformPosition;
        m_TeleportAnimation.transform.localScale = new Vector3(1f, stretchSize, 1f);
        yield return new WaitForSeconds(0.125f);
        m_TeleportAnimation.SetActive(false);
        m_renderer.enabled = true;
    }
}
