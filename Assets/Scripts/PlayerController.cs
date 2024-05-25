using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    Rigidbody2D rb;
    [SerializeField] float speed = 1.0f;
    [SerializeField] float speedMultiplier = 2.0f;
    [SerializeField] float jumpPower = 1.0f;

    
    private bool isGrounded;
    private bool isJumping = false;
    private float originalGravityScale;
    public Transform groundCheck;   // �v���C���[�𒆐S�ɉ~��u���A�ݒu����ɗp����.
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;   // �n�ʂ̃I�u�W�F�N�g�ɂɐݒ肵�Ă������C���[.
    public float increasedGravityScale = 2.0f;  // �d�͑����̔{��.Jump�L�[�������ꂽ�Ƃ��d�͂𑝉�������.

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalGravityScale = rb.gravityScale;
        rb.freezeRotation = true;
        PhysicsMaterial2D physicsMaterial2D = new PhysicsMaterial2D("Player");
        physicsMaterial2D.friction = 0;
        GetComponent<BoxCollider2D>().sharedMaterial = physicsMaterial2D;

        // FixedUpdate���ł�Input.GetButtonDown����肭�󂯎��Ȃ��̂�UniTask�Ŕ񓯊�����.
        // �I�u�W�F�N�g���j�����ꂽ�Ƃ��AJumpAction�̏�����j������.
        var token = this.GetCancellationTokenOnDestroy();
        _ = JumpAction(token);

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Destroy(this.gameObject);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isGrounded)
        {
            rb.gravityScale = originalGravityScale; // �n�ʂɂ���Ƃ��͏d�͂����ɖ߂�
            isJumping = false;
        }
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        float input = Input.GetAxis("Horizontal");
        float ySpeedDecreace = rb.gravityScale * Time.deltaTime;

        // �����̕ύX����.
        if (input > 0) rb.transform.localScale = new Vector2(1, 1);
        if (input < 0) rb.transform.localScale = new Vector2(-1, 1);

        if (Mathf.Abs(input) > 0)
        {
            if (Input.GetKey(KeyCode.F)) // F�L�[�����Ƀ_�b�V���L�[�Ƃ���.
            {                
                rb.velocity = new Vector2(speed * speedMultiplier * rb.transform.localScale.x , rb.velocity.y - ySpeedDecreace);
            }
            else
            {                
                rb.velocity = new Vector2(speed * rb.transform.localScale.x, rb.velocity.y - ySpeedDecreace);
            }
        }
        else
        {
            
            rb.velocity = new Vector2(0, rb.velocity.y - ySpeedDecreace);
        }
    }

    private async UniTaskVoid JumpAction(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await UniTask.WaitUntil(() => Input.GetButtonDown("Jump"), PlayerLoopTiming.Update, token); // Jump�L�[�̓��͂�����܂� await �őҋ@.
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);  // ����FixedUpdate�̃^�C�~���O�܂� await �őҋ@.

            if (isGrounded)
            {
                rb.AddForce(Vector3.up * jumpPower, ForceMode2D.Impulse);
                isJumping = true;
            }

            if (isJumping)
            {
                // �d�͂̕ύX���s�������ɓn��token������
                var cts = new CancellationTokenSource();

                UniTask.Void(async () =>
                {
                    await UniTask.Delay(100);
                    await UniTask.WaitUntil(() => isGrounded, PlayerLoopTiming.FixedUpdate, token);
                    if (!Input.GetButton("Jump")) rb.gravityScale = increasedGravityScale;
                    cts.Cancel();
                    isJumping = false;
                    Debug.Log("Cancel");
                });

                // �X�y�[�X�L�[���������̂�ҋ@����^�X�N
                if (!cts.Token.IsCancellationRequested)
                {
                    Debug.Log("wait key up");
                    await UniTask.WaitUntil(() => Input.GetButtonUp("Jump"), PlayerLoopTiming.Update, token);
                    Debug.Log("key up");
                    if (!cts.Token.IsCancellationRequested)
                    {
                        rb.gravityScale = increasedGravityScale;
                    }
                }
            }
        }
        Debug.Log("JumpAction is End");
    }

}
