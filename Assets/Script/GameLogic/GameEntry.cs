using UnityEngine;
using UnityEngine.InputSystem;
using YBFramework.GameLogic.Input;

namespace YBFramework.GameLogic
{
    public sealed class GameEntry : MonoBehaviour, PlayerInputAction.IPlayerInputActions
    {
        private PlayerInputAction m_PlayerInputAction;

        private void Awake()
        {
            m_PlayerInputAction = new PlayerInputAction();
            m_PlayerInputAction.PlayerInput.AddCallbacks(this);
        }

        private void OnDestroy()
        {
            m_PlayerInputAction.Dispose(); // Destroy asset object.
        }

        private void OnEnable()
        {
            m_PlayerInputAction.Enable(); // Enable all actions within map.
        }

        private void OnDisable()
        {
            m_PlayerInputAction.Disable(); // Disable all actions within map.
        }

        public void OnTest(InputAction.CallbackContext context)
        {
            Debug.LogError($"{context.action.name} invoked");
        }
    }
}