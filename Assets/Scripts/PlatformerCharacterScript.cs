using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;
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
    [SerializeField] private SpriteRenderer playerSpriteRend;
    [SerializeField] private LevelScript level;

    [Header("UI Elements")]
    [SerializeField] private RectMask2D staminaMask;
    [SerializeField] private GameObject staminaDisplay;
    [SerializeField] private GameObject inventoryDisplay;

    //Private Movement Vars
    private float horizontalMove = 0;
    private float verticalMove = 0;
    private bool climbing = false;
    private bool onGround;
    private bool jumping;
    private bool endingJump;
    private bool walkingSoundIsPlaying = false;
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
    public InputAction pauseAction;

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
    [SerializeField] private AudioClip[] walkingSfx;
    [SerializeField] private AudioClip[] climbingSfx;
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

    public void ResetPlayer (Vector2 pos)
    {
        SwapMode(true);
        currentStamina = maxStaminaSeconds;
        horizontalMove = verticalMove = 0;
        rb.velocity = Vector2.zero;
        transform.position = pos;
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
        pauseAction = actions.FindActionMap("Platforming").FindAction("Pause");
        rotateAction = actions.FindActionMap("Platforming").FindAction("Rotate");
        jumpAction.performed += _ => Jump();
        climbAction = actions.FindActionMap("Platforming").FindAction("Climb");
        climbAction.performed += _ => Climb();
    }

    // Update is called once per frame
    void Update()
    {
        if (!building && !loading && !LevelScript.Instance.gamePaused)
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
        Debug.Log("OnGround: " + onGround + " " + Time.deltaTime + " Time in Air live: " + timeInAir);
        if (!onGround)
        {
            timeInAir += Time.deltaTime;

        }
        UpdateAnimatorVars();
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
        if (jumping && (!(jumpAction.ReadValue<float>() > 0) || timeBeforeDownforce < timeInAir) && !endingJump)
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
        playerAnim.SetBool("OnGround", onGround);
        playerAnim.SetFloat("WalkXVel", rb.velocity.x);
        playerAnim.SetBool("Climbing", climbing);
        playerAnim.SetFloat("VelMag", rb.velocity.magnitude);
        playerAnim.SetFloat("Stamina", currentStamina);
        playerAnim.SetBool("Building", building);
        //May need to scale magnitude
    }
    void Climb()
    {
        if (!LevelScript.Instance.gamePaused)
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
    }

    void EnableClimb ()
    {
        climbing = true;
        rb.gravityScale = 0f;
        //rb.bodyType = RigidbodyType2D.Kinematic;
    }

    public void DisableClimb(bool removeVelocity = false)
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
        
    }
    void CheckForClimbable()
    {
        List<Collider2D> overlapResults = new List<Collider2D>();
        Physics2D.OverlapCollider(grabCollider, grabContactFilter, overlapResults);
        if (overlapResults.Count == 0)
        {
            DisableClimb();
        }
    }
    void Jump () {
        if (timeInAir <= coyoteTime && !building && !loading && !LevelScript.Instance.gamePaused)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumping = true;
            timeJumpPressed = 0f;
            playerAnim.SetTrigger("Jump");
            SoundManager.Instance.PlaySFXClip(jumpSfx[Random.Range(0, jumpSfx.Length - 1)], Camera.main.transform);
        }
        else if (!onGround)
        {
            timeJumpPressed = timeInAir;
            Debug.Log(timeInAir);
        }
    }
    public void OnLeaveGround (Transform platform = null) {
        onGround = false;
        // stop walking sound
        if (platform) {
            if (platform == currentPlatform) {
                currentPlatform = null;
                transform.parent = null;
            }
        }
    }

    IEnumerator PlayWalkingSound()
    {
        AudioClip toPlay = walkingSfx[Random.Range(0, walkingSfx.Length - 1)];
        SoundManager.Instance.PlaySFXClip(toPlay, Camera.main.transform);
        yield return new WaitForSeconds(toPlay.length);
        walkingSoundIsPlaying = false;
    }
    IEnumerator PlayClimbingSound()
    {
        AudioClip toPlay = climbingSfx[Random.Range(0, walkingSfx.Length - 1)];
        SoundManager.Instance.PlaySFXClip(toPlay, Camera.main.transform);
        yield return new WaitForSeconds(toPlay.length);
        walkingSoundIsPlaying = false;
    }

    public void OnEnterGround(Transform platform = null) {
        if (!onGround)
        {
            onGround = true;
            jumping = false;
            endingJump = false;
            if (timeJumpPressed > 0f && (timeInAir - timeJumpPressed <= jumpBuffer))
            {
                timeInAir = 0;
                Jump();
            }
            timeInAir = 0;
            // Plays landing sfx
            SoundManager.Instance.PlaySFXClip(landSfx, Camera.main.transform);
            if (platform)
            {
                currentPlatform = platform;
                transform.parent = platform;
            }
        }
    }
    void ApplyDownForce() {
        rb.AddForce(Vector2.down * downForce, ForceMode2D.Impulse);
    }
    void Move () {
        
        if (!climbing)
        {
            if (!walkingSoundIsPlaying && onGround)
            {
                walkingSoundIsPlaying = true;
                StartCoroutine(PlayWalkingSound());
            }
            rb.velocity = new Vector2(horizontalMove * moveSpeed, rb.velocity.y);
        } else
        {
            if (!walkingSoundIsPlaying && climbing && rb.velocity.magnitude > 0.01f)
            {
                walkingSoundIsPlaying = true;
                StartCoroutine(PlayClimbingSound());
            }
            rb.velocity = new Vector2(horizontalMove * moveSpeed, verticalMove * moveSpeed);
        }
        
    }

    public void SwapMode(bool buildMode = false)
    {
        building = buildMode;
        inventoryDisplay.SetActive(buildMode);
        staminaDisplay.SetActive(!buildMode);
    }

    IEnumerator SetInactiveAfterTime (float time, GameObject obj)
    {
        yield return new WaitForSeconds (time);
        obj.SetActive (false);
    }
}
