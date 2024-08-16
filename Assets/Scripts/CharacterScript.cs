using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class CharacterScript : MonoBehaviour
{
    public enum PlayerMode {
        None,
        Repair,
        Dig,
        Zap

    }
    public static CharacterScript Instance { get; private set; }
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GroundDetector feet;
    [SerializeField] private Animator playerAnim;
    [SerializeField] private RadialMenu radialMenu;
    [SerializeField] private AudioSource playerAudio;
    [SerializeField] private Transform zapCastPoint;
    [SerializeField] private SpriteRenderer playerSpriteRend;

    [Header("HUD")]

    [SerializeField] private TMP_Text modeDisplay;
    [SerializeField] private HealthDisplay healthDisplay;
    

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

    [Header("Movement Vars")]
    [SerializeField] private float deadZone = 0.01f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float downForce = 2f;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float timeBeforeDownforce = 1f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBuffer = 0.15f;
    //Private Gameplay Vars
    private int health = 3;
    private bool alive = true;
    private bool invincible = false;
    private ICanBeRepaired repairInteraction;
    private ICanBeDug digInteraction;

    private bool menuActive = false;
    
    [Header("Gameplay Vars")]
    public bool[] modesUnlocked;
    [SerializeField] private bool[] modesUnlockedAtStart;
    [SerializeField] private PlayerMode currentMode = PlayerMode.None;
    [SerializeField] private Color invincibilityColor;
    [SerializeField] private TMP_Text newModeDisplay;
    [SerializeField] private GameObject deathMenu;
    [SerializeField] private Sprite deadSprite;
    [SerializeField] private float invincibilityTime = 2f;
    

    [Header("Sound Effects")]
    [SerializeField] private AudioClip[] jumpSfx;
    [SerializeField] private AudioClip landSfx;
    [SerializeField] private AudioClip[] sillySfx;
    [SerializeField] private AudioClip[] zapSfx;
    [SerializeField] private AudioClip digSfx;
    [SerializeField] private AudioClip[] fixSfx;
    [SerializeField] private AudioClip[] modeUnlockSfx;
 
    [Header("Visual Effects")]
    [SerializeField] private GameObject zapEffect;
    [SerializeField] private ParticleSystem digParticles;

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
        transform.position = LevelScript.Instance.entryPoints[DontDestroyOnLoadData.Instance.entryPoint].position;
        LoadProgress();
        for (int i = 0; i < modesUnlocked.Length; i++)
        {
            modesUnlockedAtStart[i] = modesUnlocked[i];
        }
        radialMenu.BuildMenu(modesUnlocked);
        UpdateUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (alive && !PauseScript.Instance.paused)
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
            UpdateAnimatorVars();
        }
    }
    public void LoadProgress()
    {
        currentMode = DontDestroyOnLoadData.Instance.currentPlayerMode;
        modesUnlocked = DontDestroyOnLoadData.Instance.modesUnlocked;

    }

    public void SaveProgress()
    {
        DontDestroyOnLoadData.Instance.currentPlayerMode = currentMode;
        DontDestroyOnLoadData.Instance.modesUnlocked = modesUnlocked;
    }

    void UpdateUI() {
        string modeName = "Uninitialised";
        switch (currentMode)
        {
            case PlayerMode.None:
                modeName = "legMODE";
                break;
            case PlayerMode.Repair:
                modeName = "fixMODE";
                break;
            case PlayerMode.Dig:
                modeName = "digMODE";
                break;
            case PlayerMode.Zap:
                modeName = "zapMODE";
                break;
        }
        modeDisplay.text = modeName;
        healthDisplay.UpdateDisplay(health);
    }

    void ProcessInput () {
        if (Mathf.Abs(Input.GetAxis("Horizontal")) > deadZone) {
            horizontalMove = Input.GetAxis("Horizontal");
        } else if (horizontalMove != 0) {
            horizontalMove = 0;
        }
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (timeInAir <= coyoteTime) {
                Jump();
            } else if (!onGround) {
                timeJumpPressed = timeInAir; 
            }
        }
        if (jumping && (!Input.GetKey(KeyCode.Space)|| timeBeforeDownforce < timeInAir) && !endingJump) {
            endingJump = true;
            ApplyDownForce();
        }
        if (Input.GetKeyDown(KeyCode.Q)) {
            ToggleRadialMenu();
        }
        if (Input.GetKeyDown(KeyCode.E)) {
            DoModeAction();
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            UnlockMode((PlayerMode)Random.Range(0, 4));
        }
    }
    void DoModeAction () {
        switch (currentMode)
        {
            case PlayerMode.None:
                playerOneShotAudio.PlayOneShot(sillySfx[Random.Range(0, sillySfx.Length)]);
                break;
            case PlayerMode.Repair:
                
                if (repairInteraction != null && !repairInteraction.repaired)
                {
                    repairInteraction.Repair();
                    playerOneShotAudio.PlayOneShot(fixSfx[Random.Range(0, fixSfx.Length)]);
                }
                break;
            case PlayerMode.Dig:
                playerOneShotAudio.PlayOneShot(digSfx);
                if (digInteraction != null)
                {
                    digInteraction.Dig();
                    StartCoroutine(FreezePlayer(1f));
                    digParticles.Play();
                }
                break;
            case PlayerMode.Zap:
                Zap();
                break;
        }
    }
    void OnTriggerEnter2D (Collider2D col) {
        if (col.gameObject.TryGetComponent(out ICanBeRepaired repair)) {
            repairInteraction = repair;
        } else if (col.gameObject.TryGetComponent(out ICanBeDug dig)) {
            digInteraction = dig;
        }
    }
    void OnTriggerExit2D (Collider2D col) {
        if (col.gameObject.TryGetComponent(out ICanBeRepaired repair)) {
            if (repair == repairInteraction) {
                repairInteraction = null;
            }
        } else if (col.gameObject.TryGetComponent(out ICanBeDug dig)) {
            if (dig == digInteraction) {
                digInteraction = null;
            }
        }
    }
    void UpdateAnimatorVars () {
        playerAnim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        playerAnim.SetInteger("Mode", (int)currentMode);
    }
    void Jump () {
        rb.velocity = new Vector2(rb.velocity.x,jumpForce);
        jumping = true;
        timeJumpPressed = 0f;
        playerOneShotAudio.PlayOneShot(jumpSfx[Random.Range(0,jumpSfx.Length)]);
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

    void Zap () {
        Instantiate(zapEffect, zapCastPoint.position,zapCastPoint.localRotation,zapCastPoint);
        playerOneShotAudio.PlayOneShot(zapSfx[Random.Range(0,zapSfx.Length)]);
        Collider2D[] cols = Physics2D.OverlapBoxAll(zapCastPoint.position,Vector2.one,0);
        foreach (Collider2D col in cols) {
            if (col.gameObject.TryGetComponent(out ICanBeZapped zappable)) {
                zappable.GetZapped();
            }
        }
    }
    void Flip() {
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
    }
    public void UnlockMode (PlayerMode mode)
    {
        modesUnlocked[(int)mode] = true;
        newModeDisplay.gameObject.SetActive(true);
        string modeName = "Uninitialised";
        switch (mode)
        {
            case PlayerMode.None:
                modeName = "legMODE";
                break;
            case PlayerMode.Repair:
                modeName = "fixMODE";
                break;
            case PlayerMode.Dig:
                modeName = "digMODE";
                break;
            case PlayerMode.Zap:
                modeName = "zapMODE";
                break;
        }
        newModeDisplay.text = modeName + " UNLOCKED!";
        StartCoroutine(SetInactiveAfterTime(4f,newModeDisplay.gameObject));
        AudioSource.PlayClipAtPoint(modeUnlockSfx[(int)mode - 1], transform.position, 0.5f);
        radialMenu.BuildMenu(modesUnlocked);
    }

    IEnumerator SetInactiveAfterTime (float time, GameObject obj)
    {
        yield return new WaitForSeconds (time);
        obj.SetActive (false);
    }
    public void SwapMode (PlayerMode mode) {
        currentMode = mode;
        UpdateUI();
        Debug.Log("mode = " + mode);
    }
    public void ToggleRadialMenu () {
        menuActive = !menuActive;
        radialMenu.gameObject.SetActive(menuActive);
    }

    public void TakeDamage(int amount) {
        if (!invincible && alive)
        {
            health -= amount;
            UpdateUI();
            if (health <= 0)
            {
                Die();
            } else
            {
                StartCoroutine(StartInvincibility());
            }
        }
    }

    private IEnumerator FreezePlayer(float timeToFreezeFor)
    {
        rb.simulated = alive = false;
        yield return new WaitForSeconds(timeToFreezeFor);
        rb.simulated = alive = true;
    }


    public void Die() {
        for (int i = 0; i < modesUnlockedAtStart.Length; i++)
        {
            DontDestroyOnLoadData.Instance.modesUnlocked[i] = modesUnlockedAtStart[i];
        }
        playerOneShotAudio.PlayOneShot(sillySfx[6]);
        DontDestroyOnLoadData.Instance.modesUnlocked = modesUnlockedAtStart;
        deathMenu.SetActive(true);
        Destroy(playerAnim);
        playerSpriteRend.sprite = deadSprite;
        alive = false;
    }

    IEnumerator StartInvincibility ()
    {
        invincible = true;
        playerSpriteRend.color = invincibilityColor;
        yield return new WaitForSeconds(invincibilityTime / 5);
        playerSpriteRend.color = Color.white;
        yield return new WaitForSeconds(invincibilityTime / 5);
        playerSpriteRend.color = invincibilityColor;
        yield return new WaitForSeconds(invincibilityTime / 5);
        playerSpriteRend.color = Color.white;
        yield return new WaitForSeconds(invincibilityTime / 5);
        playerSpriteRend.color = invincibilityColor;
        yield return new WaitForSeconds(invincibilityTime / 5);
        playerSpriteRend.color = Color.white;
        invincible = false;
    }
}
