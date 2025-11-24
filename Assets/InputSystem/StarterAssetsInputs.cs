using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool shoot;
		public bool zoom;
		public bool inputBlocked = false;
		public bool pause;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

		void Start()
		{
			SetCursorState(true);
		}

		public void SetInputBlocked(bool state)
		{
			inputBlocked = state;

			if (state)
			{
				move = Vector2.zero;
				look = Vector2.zero;
				jump = false;
				sprint = false;
				shoot = false;
				zoom = false;
			}
		}

#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			if (inputBlocked) return;
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if (inputBlocked) return;
			if (cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			if (inputBlocked) return;
			SprintInput(value.isPressed);
		}

		public void OnShoot(InputValue value)
		{
			if (inputBlocked) return;
			ShootInput(value.isPressed);
		}

		public void OnZoom(InputValue value)
		{
			if (inputBlocked) return;
			ZoomInput(value.isPressed);
		}
		public void OnPause(InputValue value)
		{
			PauseInput(value.isPressed);
		}
#endif

		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		}

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}
		public void ShootInput(bool newShootState)
		{
			shoot = newShootState;
		}

		public void ZoomInput(bool newZoomState)
		{
			zoom = newZoomState;
		}

		public void PauseInput(bool newPauseState)
		{
			pause = newPauseState;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		public void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}

}