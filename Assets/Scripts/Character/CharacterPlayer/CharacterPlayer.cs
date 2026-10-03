using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterPlayer : CharacterBase
{
    public static CharacterPlayer Instance { get; private set; }
    PlayerInputs playerInputs;
    public Rigidbody2D rb;
    public int direction = 1;
    public Animator animator;
    public bool _isGrounded = false;
    public bool isGrounded
    {
        get => _isGrounded;
        set
        {
            if (!_isGrounded && value)
            {
                jumpCount = 1;
            }
            _isGrounded = value;
        }
    }
    bool firstTouch;
    public int jumpCount;
    public float jumpForce;
    public float speed;
    public Vector2 boxSize;
    public Vector2 boxOffset;
    public Vector2 boxOffsetGrounded;
    public Vector2 boxSizeGrounded;
    public float distance;
    public bool obtainedGear;
    public Action OnPlayerDie;
    public void Awake()
    {
        Instance = this;
    }
    public override void InitializeCharacter()
    {
        base.InitializeCharacter();
        movementDirection = Vector2.right;
        rb.constraints = RigidbodyConstraints2D.None;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
        playerInputs = new PlayerInputs();
        playerInputs.Player.Enable();
        playerInputs.Player.TouchPosition.Enable();
        playerInputs.Player.ScreenPress.Enable();
        playerInputs.Player.ScreenPress.started += OnHandleTouchScreen;
        playerInputs.Player.ScreenPress.canceled += OnHandleTouchScreen;
    }
    void OnDisable()
    {
        playerInputs.Player.TouchPosition.Disable();
        playerInputs.Player.ScreenPress.Disable();
        playerInputs.Player.ScreenPress.started -= OnHandleTouchScreen;
        playerInputs.Player.ScreenPress.canceled -= OnHandleTouchScreen;
        playerInputs.Player.Disable();
    }
    void Update()
    {
        if (isSctive)
        {
            print(Time.timeScale);
            rb.linearVelocity = new Vector2(movementDirection.x * speed, rb.linearVelocity.y);
            ValidateGroundType();
        }
    }
    public override void HandleStart()
    {
        isSctive = true;
        movementDirection = Vector2.right;
        rb.constraints = RigidbodyConstraints2D.None;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
    }
    public override void HandleDie()
    {
        base.HandleDie();
        firstTouch = false;
        rb.constraints = RigidbodyConstraints2D.FreezePositionX;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
    }
    public void ValidateGroundType()
    {
        CheckCollisionGrounded(out RaycastHit2D collisionGrounded);
        CheckCollisionLeft(out RaycastHit2D collisionLeft);
        CheckCollisionRight(out RaycastHit2D collisionRight);
        isGrounded = !!collisionGrounded;
        if (!!collisionGrounded)
        {
            collisionGrounded.collider.GetComponent<MapBase>().OnHandlePlayerEnter(this, MapBase.TypeDirection.Down);
        }
        if (!!collisionLeft)
        {
            collisionLeft.collider.GetComponent<MapBase>().OnHandlePlayerEnter(this, MapBase.TypeDirection.Left);
        }
        if (!!collisionRight)
        {
            collisionRight.collider.GetComponent<MapBase>().OnHandlePlayerEnter(this, MapBase.TypeDirection.Right);
        }
    }
    public void CheckCollisionGrounded(out RaycastHit2D collisionGrounded)
    {
        RaycastHit2D hitGround = Physics2D.BoxCast(transform.position + (Vector3)boxOffsetGrounded, boxSizeGrounded, 0f, Vector2.down, distance, LayerMask.GetMask("Ground"));
        collisionGrounded = hitGround;
    }
    void OnHandleTouchScreen(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.startGame && !LevelManager.Instance.finishGame)
        {
            if (context.started)
            {
                if (!isSctive)
                {
                    HandleStart();
                    return;
                }
                animator.SetBool("ChargeJump", true);
            }
            else if (context.canceled)
            {
                if (!firstTouch)
                {
                    firstTouch = true;
                    return;
                }
                animator.SetBool("ChargeJump", false);
                if (jumpCount > 0) JumpToPosition();
            }
        }
    }
    public void CheckCollisionLeft(out RaycastHit2D collisionLeft)
    {
        RaycastHit2D hitLeft = Physics2D.BoxCast(transform.position + (Vector3)boxOffset, boxSize, 0f, Vector2.left, distance, LayerMask.GetMask("Ground"));
        collisionLeft = hitLeft;
    }
    public void CheckCollisionRight(out RaycastHit2D collisionRight)
    {
        RaycastHit2D hitRight = Physics2D.BoxCast(transform.position + (Vector3)boxOffset, boxSize, 0f, Vector2.right, distance, LayerMask.GetMask("Ground"));
        collisionRight = hitRight;
    }
    void JumpToPosition()
    {
        jumpCount--;
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }
    void OnDrawGizmos()
    {
        DrawBoxCastGizmo(Vector2.right, Color.green, boxSize, boxOffset);
        DrawBoxCastGizmo(Vector2.left, Color.blue, boxSize, boxOffset);
        DrawBoxCastGizmo(Vector2.down, Color.red, boxSizeGrounded, boxOffsetGrounded);
    }

    void DrawBoxCastGizmo(Vector2 direction, Color color, Vector2 size, Vector2 offset)
    {
        Gizmos.color = color;
        Vector2 origin = (Vector2)transform.position + offset;
        Vector2 end = origin + direction.normalized * distance;
        Gizmos.DrawWireCube(end, size);
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Gear") && !obtainedGear)
        {
            obtainedGear = true;
            Gear.Instance.gearSprite.SetActive(false);
            Gear.Instance.OnHandleObtainGear?.Invoke();
        }
        else if (collision.CompareTag("Portal") && obtainedGear)
        {
            LevelManager.Instance.OnHandleLevelFinished();
        }
        else if (collision.CompareTag("Enemy"))
        {
            LevelManager.Instance.OnHandlePlayerDied();
            OnPlayerDie?.Invoke();
        }
        else if (collision.CompareTag("Orb"))
        {
            collision.GetComponent<OrbBase>().OnPlayerEnter(this);
        }
    }
}
