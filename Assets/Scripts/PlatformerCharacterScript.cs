using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlatformerCharacterScript : MonoBehaviour
{
    public static PlatformerCharacterScript Instance { get; private set; }
    public InputActionAsset actions;
    public bool building = true;
    public bool loading = true;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GroundDetector feet;
    [SerializeField] private Collider2D grabCollider;
    [SerializeField] private Animator playerAnim;
    [SerializeField] private AudioSource playerAudio;
    [SerializeField] private SpriteRenderer playerSpriteRend;
    [SerializeField] private AudioSource playerOneShotAudio;
    [SerializeField] private LevelScript level;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text debugStaminaText;
    [SerializeField] private RectMask2D staminaMask;
    [SerializeField] private GameObject staminaDisplay;
    [SerializeField] private GameObject inventoryDisplay;

    //Private Movement Vars
    private float horizontalMove = 0;
    private float verticalMove = 0;
    private bool facingRight = true;
    private bool climbing = false;
    private bool onGround;
    private bool jumping;
    private bool endingJump;
    private float timeJumpPressed = 0f;
    private float timeInAir = 0f;
    private float currentStamina = 3f;
    private float staminaMaskMaxY;
    private Transform currentPlatform;

    //Input Actions
    public InputAction moveAction;
    public InputAction jumpAction;
    public InputAction climbAction;
    public InputAction placeAction;
    public InputAction placeMouseAction;
    public InputAction rotateAction;
    public InputAction swapModeAction;
    public InputAction cursorUpAction;
    public InputAction cursorDownAction;
    public InputAction cancelAction;

    [Header("Movement Vars")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float downForce = 2f;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float timeBeforeDownforce = 1f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBuffer = 0.15f;
    [SerializeField] private float maxStaminaSeconds = 3f;
    public ContactFilter2D grabContactFilter;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip[] jumpSfx;
    [SerializeField] private AudioClip landSfx;
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        staminaMaskMaxY = staminaMask.rectTransform.rect.height;
        InitialiseInputActions();
        LoadProgress();
        UpdateUI();
    }

    public void ResetPlayer (Vector2 pos, bool rightFacing)
    {
        SwapMode(true);
        currentStamina = maxStaminaSeconds;
        rb.velocity = Vector2.zero;
        transform.position = pos;
        facingRight = rightFacing;
    }

    private void InitialiseInputActions()
    {
        actions.FindActionMap("Platforming").Enable();
        moveAction = actions.FindActionMap("Platforming").FindAction("Move");
        placeAction = actions.FindActionMap("Platforming").FindAction("Place");
        placeMouseAction = actions.FindActionMap("Platforming").FindAction("PlaceMouse");
        cancelAction = actions.FindActionMap("Platforming").FindAction("Cancel");
        jumpAction = actions.FindActionMap("Platforming").FindAction("Jump");
        swapModeAction = actions.FindActionMap("Platforming").FindAction("Swap Mode");
        cursorUpAction = actions.FindActionMap("Platforming").FindAction("CursorUp");
        cursorDownAction = actions.FindActionMap("Platforming").FindAction("CursorDown");
        rotateAction = actions.FindActionMap("Platforming").FindAction("Rotate");
        jumpAction.performed += _ => Jump();
        climbAction = actions.FindActionMap("Platforming").FindAction("Climb");
        climbAction.performed += _ => Climb();
    }

    // Update is called once per frame
    void Update()
    {
        if (!building && !loading)
        {
            ProcessInput();
        }
        if (climbing)
        {
            Move();
            CheckForClimbable();
        }
        ChangeStamina();
        if (horizontalMove != 0)
        {
            Move();
        }
        if (horizontalMove != 0 || verticalMove != 0)
        {
            
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
        staminaMask.padding = new Vector4(staminaMask.padding.x, staminaMaskMaxY - ((staminaMaskMaxY / 100) * ((currentStamina / maxStaminaSeconds) * 100)), staminaMask.padding.z, staminaMask.padding.w);
    }

    void ProcessInput()
    {
        horizontalMove = moveAction.ReadValue<Vector2>().x;
        verticalMove = moveAction.ReadValue<Vector2>().y;
        if (jumping && (!Input.GetKey(KeyCode.W) || timeBeforeDownforce < timeInAir) && !endingJump)
        {
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
    void Climb()
    {
        if (!building && !loading && !climbing && currentStamina > 0)
        {
            List<Collider2D> overlapResults = new List<Collider2D>();
            Physics2D.OverlapCollider(grabCollider, grabContactFilter, overlapResults);
            if (overlapResults.Count > 0)
            {
                EnableClimb();
            }

        }
        else
        {
            DisableClimb();
        }
    }

    void EnableClimb ()
    {
        climbing = true;
        rb.gravityScale = 0f;
        //rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void DisableClimb(bool removeVelocity = false)
    {
        climbing = false;
        rb.gravityScale = 1f;
        //rb.bodyType = RigidbodyType2D.Dynamic;
        if (removeVelocity)
        {
            rb.velocity = Vector2.zero;
        }
    }

    void ChangeStamina ()
    {
        if (climbing)
        {
            currentStamina -= Time.deltaTime * Mathf.Max(new Vector2(horizontalMove,verticalMove).magnitude,0.1f);
            if (currentStamina <= 0)
            {
                currentStamina = 0;
                DisableClimb();
            }
        } else if (onGround && currentStamina < maxStaminaSeconds)
        {
            currentStamina += Time.deltaTime;
        }
        if (currentStamina > maxStaminaSeconds)
        {
            currentStamina = 3f;
        }
        UpdateUI();
        debugStaminaText.text = currentStamina.ToString();
    }
    void CheckForClimbable()
    {
        List<Collider2D> overlapResults = new List<Collider2D>();
        Physics2D.OverlapCollider(grabCollider, grabContactFilter, overlapResults);
        if (overlapResults.Count == 0)
        {
            DisableClimb(true);
        }
    }
    void Jump () {
        if (timeInAir <= coyoteTime && !building && !loading)
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
        if (!climbing)
        {
            rb.velocity = new Vector2(horizontalMove * moveSpeed, rb.velocity.y);
            if ((horizontalMove < 0 && facingRight) || (horizontalMove > 0 && !facingRight))
            {
                Flip();
            }
        } else
        {
            rb.velocity = new Vector2(horizontalMove * moveSpeed, verticalMove * moveSpeed);
        }
        
    }

    public void SwapMode(bool buildMode = false)
    {
        building = buildMode;
        inventoryDisplay.SetActive(buildMode);
        staminaDisplay.SetActive(!buildMode);
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
