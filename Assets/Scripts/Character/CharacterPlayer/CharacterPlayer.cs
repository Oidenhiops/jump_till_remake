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
    public float currentSpeed;
    public Vector2 boxSize;
    public Vector2 boxOffset;
    public Vector2 boxOffsetGrounded;
    public Vector2 boxSizeGrounded;
    public float distance;
    public bool obtainedGear;
    public Action OnPlayerDie;
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
        return GameManager.Instance.startGame && !LevelManager.Instance.finishGame && isSctive;
    }
    void Update()
    {
        if (isSctive)
        {
            print(Time.timeScale);
            rb.linearVelocity = new Vector2(movementDirection.x * currentSpeed, rb.linearVelocity.y);
            ValidateGroundType();
        }
    }
    void FixedUpdate()
    {
        if (isSctive)
        {
            animator.SetBool("IsMoving", rb.linearVelocityX != 0);
            animator.SetBool("IsGrounded", isGrounded);
        }
    }
    public override void HandleStart()
    {
        isSctive = true;
        movementDirection = Vector2.right;
        currentSpeed = speed;
        rb.constraints = RigidbodyConstraints2D.None;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
    }
    public override void HandleDie()
    {
        base.HandleDie();
        currentSpeed = 0;
        firstTouch = false;
        rb.constraints = RigidbodyConstraints2D.FreezePositionX;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
    }
    public void ValidateGroundType()
    {
        collisionDown = CheckCollisionGrounded();
        collisionLeft = CheckCollisionLeft();
        collisionRight = CheckCollisionRight();
        isGrounded = !!collisionDown;
    }
    public MapBase CheckCollisionGrounded()
    {
        RaycastHit2D hitGround = Physics2D.BoxCast(transform.position + (Vector3)boxOffsetGrounded, boxSizeGrounded, 0f, Vector2.down, distance, LayerMask.GetMask("Ground"));
        return hitGround.collider?.GetComponent<MapBase>();
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
    public MapBase CheckCollisionLeft()
    {
        RaycastHit2D hitLeft = Physics2D.BoxCast(transform.position + (Vector3)boxOffset, boxSize, 0f, Vector2.left, distance, LayerMask.GetMask("Ground"));
        return hitLeft.collider?.GetComponent<MapBase>();
    }
    public MapBase CheckCollisionRight()
    {
        RaycastHit2D hitRight = Physics2D.BoxCast(transform.position + (Vector3)boxOffset, boxSize, 0f, Vector2.right, distance, LayerMask.GetMask("Ground"));
        return hitRight.collider?.GetComponent<MapBase>();
    }
    void JumpToPosition()
    {
        jumpCount--;
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        AudioManager.Instance.PlayASound(AudioManager.Instance.GetAudioClip(SoundsDBSO.TypeSound.SFX, "PlayerJump"));
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
            Gear.Instance.OnPlayerEnter();
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
