using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace MiniIT.ARKANOID
{
    public class PaddleController : MonoBehaviour
    {
        [Header("Движение по оси X")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float minX = -7.5f;
        [SerializeField] private float maxX = 7.5f;

        [Header("Тач-управление")]
        [Tooltip("Насколько сильно движение пальца по экрану влияет на движение платформы.")]
        [SerializeField] private float touchMoveSpeed = 20f;

        private GameInputActions inputActions = null;
        private float moveInput = 0f;

        private void Awake()
        {
            inputActions = new GameInputActions();
            EnhancedTouchSupport.Enable();
        }

        private void OnEnable()
        {
            inputActions.PlayerControls.Enable();
            inputActions.PlayerControls.Move.performed += OnMovePerformed;
            inputActions.PlayerControls.Move.canceled += OnMoveCanceled;
        }

        private void OnDisable()
        {
            inputActions.PlayerControls.Move.performed -= OnMovePerformed;
            inputActions.PlayerControls.Move.canceled -= OnMoveCanceled;
            inputActions.PlayerControls.Disable();
        }

        private void OnDestroy()
        {
            EnhancedTouchSupport.Disable();
        }

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<float>();
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            moveInput = 0f;
        }

        private void Update()
        {
            float deltaX = 0f;

            if (Application.isMobilePlatform && Touch.activeTouches.Count > 0)
            {
                Touch primaryTouch = Touch.activeTouches[0];
                float normalizedDelta = primaryTouch.delta.x / Screen.width;
                deltaX = normalizedDelta * touchMoveSpeed;
            }
            else
            {
                if (Mathf.Abs(moveInput) > 0.001f)
                {
                    deltaX = moveInput * moveSpeed * Time.deltaTime;
                }
            }

            if (Mathf.Abs(deltaX) > 0.0001f)
            {
                Vector3 position = transform.position;
                position.x += deltaX;
                position.x = Mathf.Clamp(position.x, minX, maxX);
                transform.position = position;
            }
        }
    }
}
