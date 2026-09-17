using UnityEngine;
using UnityEngine.InputSystem;

public class MouseGlobalPosition : MonoBehaviour
{
	[SerializeField] InputActionAsset _inputAsset = null;
	private InputAction _mouseRightButtonInput = null;
	private InputAction _mousePositionInput = null;


	private void Awake()
	{
		if (_inputAsset == null)
		{
			Debug.LogException(new System.NullReferenceException($"InputActionMap _inputActionAsset is null."), this);
		}
		else
		{
			_mouseRightButtonInput = _inputAsset.FindAction("MouseRightButton");
			_mousePositionInput = _inputAsset.FindAction("MousePosition");
		}
	}

	private void OnEnable()
	{
		_mouseRightButtonInput.performed += OnMouseRightClick;
	}

	private void OnDisable()
	{
		_mouseRightButtonInput.performed -= OnMouseRightClick;
	}


	private void OnMouseRightClick(InputAction.CallbackContext context)
	{
		Vector3 mousePosition = _mousePositionInput.ReadValue<Vector2>();
		mousePosition.z = 0f - Camera.main.transform.position.z;

		Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

		Debug.Log($"Mouse in world position: {worldPosition}");
	}
}
