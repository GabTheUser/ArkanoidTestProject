using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace MiniIT.ARKANOID
{
    public class BallController : MonoBehaviour
    {
        [Header("Настройки запуска")]
        [SerializeField] private float launchSpeed = 0f;
        [SerializeField] private float gravityScaleOnLaunch = 1f;
        [SerializeField] private float xDirectionOffsetOnLaunch = 0.5f;
        [SerializeField] private Transform paddleTransform = null;
        [SerializeField] private Vector2 startOffset = Vector2.zero;
        [SerializeField] private float minComponentThreshold = 0.2f;
        [SerializeField] private float minComponentValue = 0.3f;
        [SerializeField] private AudioClip launchSound = null;
        [SerializeField] private GameManager gameManager = null;

        [Header("Бафф и статы")]
        [SerializeField] private int baseDamage = 1;
        [SerializeField] private GameObject bonusDamageIndicator = null;

        [Header("Аудио")]
        [SerializeField] private AudioClip wallHitSound = null;
        [SerializeField] private AudioClip paddleHitSound = null;
        [SerializeField] private float wallHitVolume = 1f;

        [Header("Отладка")]
        [SerializeField] private bool allowDebugLaunchWithInput = false;

        private int bonusDamage = 0;
        private float damageBuffTimer = 0f;

        private Rigidbody2D rb = null;
        private bool isLaunched = false;

        private GameInputActions inputActions = null;

        public int CurrentDamage
        {
            get { return baseDamage + bonusDamage; }
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();

            if (rb == null)
            {
                Debug.LogError("[BallController] Не найден Rigidbody2D на объекте мяча.");
            }
            else
            {
                rb.gravityScale = 0f;
            }

            inputActions = new GameInputActions();
        }

        private void OnEnable()
        {
            inputActions.PlayerControls.Enable();

            if (allowDebugLaunchWithInput)
            {
                inputActions.PlayerControls.Launch.performed += OnLaunchPerformed;
            }
        }

        private void OnDisable()
        {
            if (allowDebugLaunchWithInput)
            {
                inputActions.PlayerControls.Launch.performed -= OnLaunchPerformed;
            }

            inputActions.PlayerControls.Disable();
        }

        private void Start()
        {
            AttachToPaddle();
        }

        private void Update()
        {
            if (!isLaunched && paddleTransform != null)
            {
                Vector3 position = paddleTransform.position;
                position += (Vector3)startOffset;
                transform.position = position;
            }

            if (damageBuffTimer > 0f)
            {
                damageBuffTimer -= Time.deltaTime;

                if (damageBuffTimer <= 0f)
                {
                    damageBuffTimer = 0f;
                    bonusDamage = 0;

                    if (bonusDamageIndicator != null)
                    {
                        bonusDamageIndicator.SetActive(false);
                    }
                }
            }

            if (!isLaunched &&
                gameManager != null &&
                gameManager.CurrentState == GameState.Playing &&
                Touchscreen.current != null)
            {
                TouchControl primaryTouch = Touchscreen.current.primaryTouch;

                if (primaryTouch.press.wasReleasedThisFrame)
                {
                    Launch();
                }
            }
        }

        private void OnLaunchPerformed(InputAction.CallbackContext context)
        {
            if (!isLaunched)
            {
                Launch();
            }
        }

        public void Launch()
        {
            if (isLaunched || rb == null)
            {
                return;
            }

            if (gameManager != null && gameManager.CurrentState != GameState.Playing)
            {
                return;
            }

            isLaunched = true;

            if (launchSound != null)
            {
                AudioSource.PlayClipAtPoint(launchSound, transform.position, wallHitVolume);
            }

            float xDirection = Random.Range(-xDirectionOffsetOnLaunch, xDirectionOffsetOnLaunch);
            Vector2 direction = new Vector2(xDirection, 1f).normalized;

            rb.linearVelocity = direction * launchSpeed;
            rb.gravityScale = gravityScaleOnLaunch;
        }

        //Бафф урона
        public void AddTemporaryDamageBuff(int amount, float duration)
        {
            bonusDamage += amount;

            if (bonusDamageIndicator != null)
            {
                bonusDamageIndicator.SetActive(true);
            }

            if (duration > damageBuffTimer)
            {
                damageBuffTimer = duration;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!isLaunched || rb == null)
            {
                return;
            }

            if (collision.collider.CompareTag("Wall"))
            {
                PlayWallHitSound(true);
            }
            else if (collision.collider.CompareTag("Paddle"))
            {
                PlayWallHitSound(false);
            }

            Vector2 velocity = rb.linearVelocity;

            if (Mathf.Abs(velocity.x) < minComponentThreshold)
            {
                float sign = velocity.x >= 0f ? 1f : -1f;
                if (Mathf.Abs(velocity.x) < 0.01f)
                {
                    sign = Random.value > 0.5f ? 1f : -1f;
                }

                velocity.x = sign * minComponentValue;
            }

            if (Mathf.Abs(velocity.y) < minComponentThreshold)
            {
                float sign = velocity.y >= 0f ? 1f : -1f;
                velocity.y = sign * minComponentValue;
            }

            rb.linearVelocity = velocity.normalized * launchSpeed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("DeathZone"))
            {
                return;
            }

            Debug.Log("[BallController] Мяч упал в DeathZone.");

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            isLaunched = false;

            if (gameManager != null)
            {
                gameManager.HandleBallLost();
            }
        }

        public void ResetBall()
        {
            isLaunched = false;

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0f;
            }

            AttachToPaddle();
        }

        private void AttachToPaddle()
        {
            if (paddleTransform == null)
            {
                return;
            }

            Vector3 position = paddleTransform.position;
            position += (Vector3)startOffset;
            transform.position = position;
        }

        private void PlayWallHitSound(bool isWall)
        {
            if (wallHitSound == null && paddleHitSound == null)
            {
                return;
            }

            if (isWall)
            {
                if (wallHitSound != null)
                {
                    AudioSource.PlayClipAtPoint(wallHitSound, transform.position, wallHitVolume);
                }
            }
            else
            {
                if (paddleHitSound != null)
                {
                    AudioSource.PlayClipAtPoint(paddleHitSound, transform.position, wallHitVolume);
                }
            }
        }
    }
}
