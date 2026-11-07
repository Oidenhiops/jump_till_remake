using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterPlayer : CharacterBase
{
    public static CharacterPlayer Instance { get; private set; }
    PlayerInputs playerInputs;
    public Rigidbody2D rb;
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
    public float currentSpeed;
    public Vector2 boxSize;
    public float distance;
    public bool obtainedGear;
    MapBase _collisionUp;
    MapBase collisionUp
    {
        get => _collisionUp;
        set
        {
            if (_collisionUp != value)
            {
                if (value == null && _collisionUp != null || value != null && value != _collisionUp)
                {
                    _collisionUp?.OnHandlePlayerExit(this, MapBase.TypeDirection.Up);
                }
                if (value != null)
                {
                    value.OnHandlePlayerEnter(this, MapBase.TypeDirection.Up);
                }
                _collisionUp = value;
            }
        }
    }
    MapBase _collisionDown;
    MapBase collisionDown
    {
        get => _collisionDown;
        set
        {
            if (_collisionDown != value)
            {
                if (value == null && _collisionDown != null || value != null && value != _collisionDown)
                {
                    _collisionDown?.OnHandlePlayerExit(this, MapBase.TypeDirection.Down);
                }
                if (value != null)
                {
                    value.OnHandlePlayerEnter(this, MapBase.TypeDirection.Down);
                }
                _collisionDown = value;
            }
        }
    }
    MapBase _collisionLeft;
    MapBase collisionLeft
    {
        get => _collisionLeft;
        set
        {
            if (_collisionLeft != value)
            {
                if (value == null && _collisionLeft != null || value != null && value != _collisionLeft)
                {
                    _collisionLeft?.OnHandlePlayerExit(this, MapBase.TypeDirection.Left);
                }
                if (value != null)
                {
                    value.OnHandlePlayerEnter(this, MapBase.TypeDirection.Left);
                }
                _collisionLeft = value;
            }
        }
    }
    MapBase _collisionRight;
    MapBase collisionRight
    {
        get => _collisionRight;
        set
        {
            if (_collisionRight != value)
            {
                if (value == null && _collisionRight != null || value != null && value != _collisionRight)
                {
                    _collisionRight?.OnHandlePlayerExit(this, MapBase.TypeDirection.Right);
                }
                if (value != null)
                {
                    value.OnHandlePlayerEnter(this, MapBase.TypeDirection.Right);
                }
                _collisionRight = value;
            }
        }
    }
    public void Awake()
    {
        Instance = this;
    }
    public override void InitializeCharacter()
    {
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
        isActive = true;
    }
    void OnEnable()
    {
        if (playerInputs != null) playerInputs.Player.Enable();
    }
    void OnDisable()
    {
        playerInputs.Player.Disable();
    }
    void OnDestroy()
    {
        playerInputs.Player.ScreenPress.started -= OnHandleTouchScreen;
        playerInputs.Player.ScreenPress.canceled -= OnHandleTouchScreen;
        playerInputs.Dispose();
        if (Instance == this) Instance = null;
    }
    bool CanMakeActions()
    {
        return GameManager.Instance.startGame && !LevelManager.Instance.finishGame && isActive;
    }
    void Update()
    {
        if (isActive)
        {
            rb.linearVelocity = new Vector2(movementDirection.x * currentSpeed, rb.linearVelocity.y);
            ValidateGroundType();
        }
    }
    void FixedUpdate()
    {
        if (isActive)
        {
            animator.SetBool("IsMoving", rb.linearVelocityX != 0);
            animator.SetBool("IsGrounded", isGrounded);
        }
    }
    public override void HandleStart()
    {
        isActive = true;
        movementDirection = Vector2.right;
        currentSpeed = speed;
        rb.constraints = RigidbodyConstraints2D.None;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
        firstTouch = true;
    }
    public override void HandleDie()
    {
        currentSpeed = 0;
        isGravityChange = false;
        firstTouch = false;
        sprite.flipX = false;
        transform.position = spawnPos.position;
        animator.SetBool("ChargeJump", false);
        animator.Play("Idle", 0, -1f);
        transform.localRotation = Quaternion.Euler(Vector3.zero);
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezePositionX;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        obtainedGear = false;
        Gear.Instance.gearSprite.enabled = true;
        OnCharacterDie?.Invoke();
    }
    public void ValidateGroundType()
    {
        collisionLeft = CheckCollisionLeft();
        collisionRight = CheckCollisionRight();
        if (!isGravityChange)
        {
            collisionDown = CheckCollisionDown();
            isGrounded = !!collisionDown;
        }
        else
        {
            collisionUp = CheckCollisionUp();
            isGrounded = !!collisionUp;
        }
    }
    void OnHandleTouchScreen(InputAction.CallbackContext context)
    {
        if (CanMakeActions())
        {
            if (context.started)
            {
                if (!firstTouch) return;
                animator.SetBool("ChargeJump", true);
            }
            else if (context.canceled)
            {
                if (!firstTouch)
                {
                    if (isGrounded) HandleStart();
                    return;
                }
                animator.SetBool("ChargeJump", false);
                if (jumpCount > 0) JumpToPosition();
            }
        }
    }
    public MapBase CheckCollisionUp()
    {
        RaycastHit2D hitGround = Physics2D.BoxCast(transform.position, new Vector2(boxSize.y, boxSize.x), 0f, Vector2.up, distance, LayerMask.GetMask("Ground"));
        return hitGround.collider?.GetComponent<MapBase>();
    }
    public MapBase CheckCollisionDown()
    {
        RaycastHit2D hitGround = Physics2D.BoxCast(transform.position, new Vector2(boxSize.y, boxSize.x), 0f, Vector2.down, distance, LayerMask.GetMask("Ground"));
        return hitGround.collider?.GetComponent<MapBase>();
    }
    public MapBase CheckCollisionLeft()
    {
        RaycastHit2D hitLeft = Physics2D.BoxCast(transform.position, boxSize, 0f, Vector2.left, distance, LayerMask.GetMask("Ground"));
        return hitLeft.collider?.GetComponent<MapBase>();
    }
    public MapBase CheckCollisionRight()
    {
        RaycastHit2D hitRight = Physics2D.BoxCast(transform.position, boxSize, 0f, Vector2.right, distance, LayerMask.GetMask("Ground"));
        return hitRight.collider?.GetComponent<MapBase>();
    }
    void JumpToPosition()
    {
        jumpCount--;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce((!isGravityChange ? Vector2.up : Vector2.down) * jumpForce, ForceMode2D.Impulse);
        AudioManager.Instance.PlayASound(AudioManager.Instance.GetAudioClip(SoundsDBSO.TypeSound.SFX, "PlayerJump"));
    }
    void OnDrawGizmos()
    {
        DrawBoxCastGizmo(Vector2.right, Color.green, boxSize, Vector2.zero);
        DrawBoxCastGizmo(Vector2.left, Color.blue, boxSize, Vector2.zero);
        DrawBoxCastGizmo(!isGravityChange ? Vector2.down : Vector2.up, Color.red, new Vector2(boxSize.y, boxSize.x), Vector2.zero);
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
            Gear.Instance.OnPlayerEnter();
        }
        else if (collision.CompareTag("Portal") && obtainedGear)
        {
            LevelManager.Instance.OnHandleLevelFinished();
        }
        else if (collision.CompareTag("Enemy"))
        {
            HandleDie();
        }
        else if (collision.CompareTag("Orb"))
        {
            collision.GetComponent<OrbBase>().OnPlayerEnter(this);
        }
    }
}
