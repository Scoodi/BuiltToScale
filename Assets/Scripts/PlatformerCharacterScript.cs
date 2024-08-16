using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class PlatformerCharacterScript : MonoBehaviour
{
    public static PlatformerCharacterScript Instance { get; private set; }
    [SerializeField] private InputActionAsset actions;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GroundDetector feet;
    [SerializeField] private Animator playerAnim;
    [SerializeField] private AudioSource playerAudio;
    [SerializeField] private SpriteRenderer playerSpriteRend;
    [SerializeField] private AudioSource playerOneShotAudio;

    //Private Movement Vars
    private float horizontalMove = 0;
    private bool facingRight = true;
    private bool onGround;
    private bool jumping;
    private bool endingJump;
    private float timeJumpPressed = 0f;
    private float timeInAir = 0f;
    private Transform currentPlatform;

    //Private Input Actions
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction climbAction;

    [Header("Movement Vars")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float downForce = 2f;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float timeBeforeDownforce = 1f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBuffer = 0.15f;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip[] jumpSfx;
    [SerializeField] private AudioClip landSfx;
    // Start is called before the first frame update
    void Start()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        InitialiseInputActions();
        LoadProgress();
        UpdateUI();
    }

    private void InitialiseInputActions()
    {
        actions.FindActionMap("Platforming").Enable();
        moveAction = actions.FindActionMap("Platforming").FindAction("Move");
        jumpAction = actions.FindActionMap("Platforming").FindAction("Jump");
        jumpAction.performed += _ => Jump();
        climbAction = actions.FindActionMap("Platforming").FindAction("Climb");
    }

    // Update is called once per frame
    void Update()
    {
        ProcessInput();
        if (horizontalMove != 0)
        {
            Move();
        }
        else if (playerAudio.isPlaying)
        {
            playerAudio.Pause();
        }
        if (!onGround)
        {
            timeInAir += Time.deltaTime;
        }
        //UpdateAnimatorVars();
    }
    public void LoadProgress()
    {

    }

    public void SaveProgress()
    {

    }

    void UpdateUI() {

    }

    void ProcessInput () {
        horizontalMove = moveAction.ReadValue<Vector2>().x;
        if (jumping && (!Input.GetKey(KeyCode.W)|| timeBeforeDownforce < timeInAir) && !endingJump) {
            endingJump = true;
            ApplyDownForce();
        }
    }

    void OnTriggerEnter2D (Collider2D col) {

    }
    void OnTriggerExit2D (Collider2D col) {

    }
    void UpdateAnimatorVars () {
        playerAnim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));

    }
    void Jump () {
        if (timeInAir <= coyoteTime)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumping = true;
            timeJumpPressed = 0f;
            playerOneShotAudio.PlayOneShot(jumpSfx[Random.Range(0, jumpSfx.Length)]);
        }
        else if (!onGround)
        {
            timeJumpPressed = timeInAir;
        }
    }
    public void OnLeaveGround (Transform platform = null) {
        onGround = false;
        playerAudio.Pause();
        if (platform) {
            if (platform == currentPlatform) {
                currentPlatform = null;
                transform.parent = null;
            }
        }
    }
    public void OnEnterGround(Transform platform = null) {
        onGround = true;
        jumping = false;
        endingJump = false;
        if (timeJumpPressed > 0f && (timeInAir - timeJumpPressed <= jumpBuffer)) {
         Jump();   
        }
        timeInAir = 0;
        playerOneShotAudio.PlayOneShot(landSfx);
        if (platform) {
           currentPlatform = platform;
           transform.parent = platform;
        }
    }
    void ApplyDownForce() {
        rb.AddForce(Vector2.down * downForce, ForceMode2D.Impulse);
    }
    void Move () {
        if (!playerAudio.isPlaying && onGround)
        {
            playerAudio.Play();
        }
        rb.velocity = new Vector2(horizontalMove*moveSpeed,rb.velocity.y);
        if ((horizontalMove < 0 && facingRight) || (horizontalMove > 0 && !facingRight)) {
            Flip();
        }
    }

    void Flip() {
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
    }

    IEnumerator SetInactiveAfterTime (float time, GameObject obj)
    {
        yield return new WaitForSeconds (time);
        obj.SetActive (false);
    }
}
