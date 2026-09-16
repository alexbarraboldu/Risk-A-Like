using UnityEngine;
using UnityEngine.InputSystem;


namespace UnityTemplateProjects
{
	public class SimpleCameraController : MonoBehaviour
	{
		class CameraState
		{
			public float x;
			public float y;
			public float z;

			public void SetFromTransform(Transform t)
			{
				x = t.position.x;
				y = t.position.y;
				z = t.position.z;
			}

			public void Translate(Vector3 translation)
			{
				x += translation.x;
				y += translation.y;
				z += translation.z;
			}

			public void LerpTowards(CameraState target, float positionLerpPct, float positionYLerpPct, float rotationLerpPct = 0f)
			{
				x = Mathf.Lerp(x, target.x, positionLerpPct);
				y = Mathf.Lerp(y, target.y, positionYLerpPct);
				z = Mathf.Lerp(z, target.z, positionLerpPct);
			}

			public void UpdateTransform(Transform t)
			{
				t.position = new Vector3(x, y, z);
			}
		}

		readonly CameraState m_TargetCameraState = new CameraState();
		readonly CameraState m_InterpolatingCameraState = new CameraState();

		[Tooltip("Terrain Layer for vertical adjustment")]
		[SerializeField] private int terrainLayer;

		[Header("Movement Settings")]

		[Tooltip("Camera speed"), Range(1f, 10f)]
		[SerializeField] private float _cameraSpeed = 5f;

		//[Tooltip("Boost factor")]
		//[SerializeField] private float _boost = 3.5f;
		private float _boostSpeed = 1;

		[Tooltip("Value to mange scroll wheel direct output"), Range(0.1f, 10f)]
		public float scrollMultiplier = 2f;
		[Tooltip("Whether or not to invert our Y axis for mouse input to rotation.")]
		public bool invertY = false;

		[Tooltip("Time it takes to interpolate camera position 99% of the way to the target on X & Z axis."), Range(0.001f, 1f)]
		public float positionLerpTime = 0.2f;
		[Tooltip("Same than positionLerpTime but for Y axis."), Range(0.001f, 1f)]
		public float positionYLerpTime = 0.8f;

		#region Rotation
		//[Header("Rotation Settings")]
		//[Tooltip("X = Change in mouse position.\nY = Multiplicative factor for camera rotation.")]
		//public AnimationCurve mouseSensitivityCurve = new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 5f), new Keyframe(1f, 2.5f, 0f, 0f));

		//[Tooltip("Time it takes to interpolate camera rotation 99% of the way to the target."), Range(0.001f, 1f)]
		//public float rotationLerpTime = 0.01f;
		#endregion

		//[SerializeField, Tooltip("Minimum distance between the camera and terrain, at which the camera stops")]
		//private float minCamDistanceToTerrain = 5f;

		private Vector3 translation;

		//[SerializeField] private InputReader _inputReader = default;
		[SerializeField] private InputActionAsset _inputAsset = null;
		private InputAction _mouseScrollInput = null;
		private InputAction _moveCameraInput = null;

		private Vector3 _inputVector;
		private Vector3 _inputHeightVector;


		private void Awake()
		{
			if (_inputAsset == null)
			{
				Debug.LogException(new System.NullReferenceException($"InputActionMap _inputActionAsset is null."), this);
				OnDisable();
				return;
			}
			else
			{
				_mouseScrollInput = _inputAsset.FindAction("MouseScroll");
				_moveCameraInput = _inputAsset.FindAction("CameraMovement");
			}
		}

		private void OnEnable()
		{
			m_TargetCameraState.SetFromTransform(transform);
			m_InterpolatingCameraState.SetFromTransform(transform);

			//_inputReader.PlayerActions.ScrollWheelEvent += OnMouseScroll;
			//_inputReader.PlayerActions.MoveCameraEvent += OnMoveCamera;
			_mouseScrollInput.performed += OnMouseScroll;
			_mouseScrollInput.canceled += OnMouseScroll;
			_moveCameraInput.performed += OnMoveCamera;
			_moveCameraInput.canceled += OnMoveCamera;
		}

		private void OnDisable()
		{
			//_inputReader.PlayerActions.ScrollWheelEvent -= OnMouseScroll;
			//_inputReader.PlayerActions.MoveCameraEvent -= OnMoveCamera;
			_mouseScrollInput.performed -= OnMouseScroll;
			_mouseScrollInput.canceled -= OnMouseScroll;
			_moveCameraInput.performed -= OnMoveCamera;
			_moveCameraInput.canceled -= OnMoveCamera;
		}

		private void Update()
		{
			#region Change camera height with mouse scroll wheel & manage the minimum distance between the camera and the terrain
			/////	Hacer que haga un lerp entre la mínima altura permitida con la próxima altura hacia la altura mínima.
			/////	Osea, si la próxima distancia es más baja que la altura permitida, se hará un lerp a la altura mínima.
			////Debug.DrawRay(transform.position, Vector3.down);
			//float diffBetweenCamAndTerrain = Utils.GetDistanceByRaycast(transform.position, Vector3.down, terrainLayer, 1000f);

			//float mSDy = 0f;
			//if (!invertY) mSDy = Input.mouseScrollDelta.y * 0.6f;
			//else mSDy = -1f * (Input.mouseScrollDelta.y * 0.6f);

			////Debug.Log("MyScrollDeltaY: " + mSDy);

			//if (mSDy != 0)
			//{
			//	float terrainPosY = transform.position.y - diffBetweenCamAndTerrain;
			//	if ((transform.position.y + mSDy) > terrainPosY + minCamDistanceToTerrain)
			//	{
			//		translation += new Vector3(0f, mSDy, 0f);
			//	}
			//	else
			//	{
			//		transform.position = new Vector3(transform.position.x, terrainPosY + minCamDistanceToTerrain, transform.position.z);
			//	}
			//}
			#endregion


			translation = (_inputVector + _inputHeightVector) * ((_cameraSpeed / 10f) * _boostSpeed);

			m_TargetCameraState.Translate(translation);

			/// Framerate-independent interpolation
			/// Calculate the lerp amount, such that we get 99% of the way to our target in the specified time
			float positionLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / positionLerpTime) * Time.deltaTime);
			float positionYLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / positionYLerpTime) * Time.deltaTime);

			m_InterpolatingCameraState.LerpTowards(m_TargetCameraState, positionLerpPct, positionYLerpPct);

			m_InterpolatingCameraState.UpdateTransform(transform);

			translation = Vector3.zero;
		}


		private void OnMouseScroll(InputAction.CallbackContext context)
		{
			if (context.phase == InputActionPhase.Performed)
			{
				float value = context.ReadValue<Vector2>().y;

				Vector3 orientationInput = Vector3.zero;
				if (transform.forward == Vector3.forward)
					orientationInput = new Vector3(0f, 0f, (value) * scrollMultiplier);
				else if (transform.forward == Vector3.down)
					orientationInput = new Vector3(0f, (value) * scrollMultiplier, 0f);

				_inputHeightVector = orientationInput;
			}
			else
			{
				_inputHeightVector = Vector3.zero;
			}
		}

		private void OnMoveCamera(InputAction.CallbackContext context)
		{
			if (context.performed)
			{
				Vector2 value = context.ReadValue<Vector2>();

				_inputVector = GetInputTranslationDirection(value);
			}
			else
			{
				_inputVector = Vector3.zero;
			}
		}

		Vector3 GetInputTranslationDirection(Vector2 inputValue)
		{
			Vector3 direction = new Vector3();

			if (inputValue.y > 0)
			{
				direction += transform.forward == Vector3.forward ? Vector3.up : Vector3.forward;
			}
			if (inputValue.y < 0)
			{
				direction += transform.forward == Vector3.forward ? Vector3.down : Vector3.back;
			}
			if (inputValue.x > 0)
			{
				direction += Vector3.right;
			}
			if (inputValue.x < 0)
			{
				direction += Vector3.left;
			}

			return direction;
		}
	}
}