using UnityEngine;
using System.Collections;

namespace RTS_Cam
{
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("RTS Camera")]
    public class RTS_Camera : MonoBehaviour
    {

        #region Foldouts

#if UNITY_EDITOR

        public int lastTab = 0;

        public bool movementSettingsFoldout;
        public bool zoomingSettingsFoldout;
        public bool rotationSettingsFoldout;
        public bool distanceSettingsFoldout;
        public bool mapLimitSettingsFoldout;
        public bool targetingSettingsFoldout;
        public bool inputSettingsFoldout;

#endif

        #endregion

        private Transform m_Transform; //camera tranform

        public bool useFixedUpdate = false; //use FixedUpdate() or Update()

        #region Movement

        public float keyboardMovementSpeed = 5f; //speed with keyboard movement
        public float screenEdgeMovementSpeed = 3f; //spee with screen edge movement
        public float followingSpeed = 5f; //speed when following a target
        public float rotationSpeed = 3f;
        public float panningSpeed = 10f;
        public float mouseRotationSpeed = 10f;

        #endregion

        #region Distance

        public bool autoDistance = true;
        public LayerMask groundMask = -1; //layermask of ground or other objects that affect distance

        public float minDistance = 10f; //minimnal distance
		public float maxDistance = 1000f; //maximal distance
		public float initialDistance = 600f; // initial distance
        public float distanceDampening = 5f; 
        public float keyboardZoomingSensitivity = 2f;
        public float scrollWheelZoomingSensitivity = 25f;

        #endregion

        #region MapLimits

        public bool limitMap = true;
        public float limitX = 50f; //x limit of map
        public float limitY = 50f; //z limit of map

        #endregion

        #region Targeting

        public Transform targetFollow; //target to follow
        public Vector3 targetOffset;

        /// <summary>
        /// are we following target
        /// </summary>
        public bool FollowingTarget
        {
            get
            {
                return targetFollow != null;
            }
        }

        #endregion

        #region Input

        public bool useScreenEdgeInput = true;
        public float screenEdgeBorder = 25f;

        public bool useKeyboardInput = true;
        public string horizontalAxis = "Horizontal";
        public string verticalAxis = "Vertical";

        public bool usePanning = true;
        public KeyCode panningKey = KeyCode.Mouse0;

        public bool useKeyboardZooming = true;
        public KeyCode zoomInKey = KeyCode.E;
        public KeyCode zoomOutKey = KeyCode.Q;

        public bool useScrollwheelZooming = true;
        public string zoomingAxis = "Mouse ScrollWheel";

        public bool useKeyboardRotation = true;
        public KeyCode rotateRightKey = KeyCode.X;
        public KeyCode rotateLeftKey = KeyCode.Z;

		public bool useKeyboardTilt = true;
		public KeyCode tiltUpKey = KeyCode.T;
		public KeyCode tiltDownKey = KeyCode.G;

        public bool useMouseRotation = true;
        public KeyCode mouseRotationKey = KeyCode.Mouse1;

        private Vector2 KeyboardInput
        {
            get { return useKeyboardInput ? new Vector2(Input.GetAxis(horizontalAxis), Input.GetAxis(verticalAxis)) : Vector2.zero; }
        }

        private Vector2 MouseInput
        {
            get { return Input.mousePosition; }
        }

        private float ScrollWheel
        {
            get { return Input.GetAxis(zoomingAxis); }
        }

        private Vector2 MouseAxis
        {
            get { return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")); }
        }

        private int ZoomDirection
        {
            get
            {
                bool zoomIn = Input.GetKey(zoomInKey);
                bool zoomOut = Input.GetKey(zoomOutKey);
                if (zoomIn && zoomOut)
                    return 0;
                else if (!zoomIn && zoomOut)
                    return 1;
                else if (zoomIn && !zoomOut)
                    return -1;
                else 
                    return 0;
            }
        }

        private int RotationDirection
        {
            get
            {
                bool rotateRight = Input.GetKey(rotateRightKey);
                bool rotateLeft = Input.GetKey(rotateLeftKey);
                if (rotateLeft && rotateRight)
                    return 0;
                else if(rotateLeft && !rotateRight)
                    return -1;
                else if(!rotateLeft && rotateRight)
                    return 1;
                else 
                    return 0;
            }
        }

		private int TiltDirection
		{
			get
			{
				bool tiltUp = Input.GetKey(tiltUpKey);
				bool tiltDown = Input.GetKey(tiltDownKey);
				if (tiltUp && tiltDown)
					return 0;
				else if(tiltUp && !tiltDown)
					return -1;
				else if(!tiltUp && tiltDown)
					return 1;
				else 
					return 0;
			}
		}

        #endregion

        #region Unity_Methods

        private void Start()
        {
            m_Transform = transform;		// initial camera transform
		}

        private void Update()
        {
            if (!useFixedUpdate)
                CameraUpdate();
        }

        private void FixedUpdate()
        {
            if (useFixedUpdate)
                CameraUpdate();
        }

        #endregion

        #region RTSCamera_Methods

        /// <summary>
        /// update camera movement and rotation
        /// </summary>
        private void CameraUpdate()
        {
			if (FollowingTarget) {				// translation
				FollowTarget ();
			} else {
				Move ();
			}

            Zoom();								// process zoom
            Rotation();							// process rotation

            LimitPosition();					// limit to allowable positions
       }

        /// <summary>
        /// move camera with keyboard or with screen edge
        /// </summary>
		private void Move()						// process translation
        {
			Vector2 localMove = Vector2.zero;	// move w.r.t. viewer coord frame
			if (useKeyboardInput) {
				localMove = KeyboardInput * Time.deltaTime * keyboardMovementSpeed;
			}
        
            if (usePanning && Input.GetKey(panningKey) && MouseAxis != Vector2.zero) {
				localMove = -MouseAxis * Time.deltaTime * panningSpeed;
            }

												// compute horizontal component of forward translation
			Vector3 worldForward = transform.TransformDirection (Vector3.forward);
			Vector3 worldGroundForward = Vector3.Normalize(new Vector3 (worldForward.x, 0, worldForward.z));
			Vector3 localGroundForward = transform.InverseTransformDirection (worldGroundForward);
			transform.Translate (localMove.y * localGroundForward + localMove.x * Vector3.right);
        }

        /// <summary>
        /// calcualte distance
        /// </summary>
        private void Zoom()		// process zoom
        {
			float zoomAmount = 0;
			if (useKeyboardZooming && ZoomDirection != 0) {
				zoomAmount = 10 * ZoomDirection * Time.deltaTime * keyboardZoomingSensitivity;
			}
			if (useScrollwheelZooming && ScrollWheel != 0) {
				zoomAmount = 100 * ScrollWheel * Time.deltaTime * scrollWheelZoomingSensitivity;
			}

			transform.Translate (zoomAmount * Vector3.forward);
        }

        /// <summary>
        /// rotate camera
        /// </summary>

		private void Rotation()				// process rotation and tilt
		{
			Ray ray = new Ray (transform.position, transform.forward); // ray to screen center
			RaycastHit hit;
			Physics.Raycast (ray, out hit);
			Vector3 centerPoint = hit.point; // ground point at screen center

			if (useMouseRotation && Input.GetKey (mouseRotationKey)) {
				transform.RotateAround (centerPoint, Vector3.up, Mathf.Lerp (0, Input.GetAxis ("Mouse X") * 10 * mouseRotationSpeed, Time.deltaTime));
				transform.RotateAround (centerPoint, transform.right, Mathf.Lerp (0, -Input.GetAxis ("Mouse Y") * 10 * mouseRotationSpeed, Time.deltaTime));
			} 
			if (useKeyboardRotation && RotationDirection != 0) {
				transform.RotateAround (centerPoint, Vector3.up, Mathf.Lerp (0, RotationDirection * 10 * rotationSpeed, Time.deltaTime));
			}
			if (useKeyboardTilt && TiltDirection != 0) {
				transform.RotateAround (centerPoint, transform.right, Mathf.Lerp (0, -TiltDirection * 10 * rotationSpeed, Time.deltaTime));
			}
		}

        /// <summary>
        /// follow targetif target != null
        /// </summary>
        private void FollowTarget()
        {
            Vector3 targetPos = new Vector3(targetFollow.position.x, m_Transform.position.y, targetFollow.position.z) + targetOffset;
            m_Transform.position = Vector3.MoveTowards(m_Transform.position, targetPos, Time.deltaTime * followingSpeed);
        }

        /// <summary>
        /// limit camera position
        /// </summary>
        private void LimitPosition()
        {
			if (limitMap) {
				m_Transform.position = new Vector3 (Mathf.Clamp (m_Transform.position.x, -limitX, limitX),
					m_Transform.position.y,
					Mathf.Clamp (m_Transform.position.z, -limitY, limitY));
			}
        }

        /// <summary>
        /// set the target
        /// </summary>
        /// <param name="target"></param>
        public void SetTarget(Transform target)
        {
            targetFollow = target;
        }

        /// <summary>
        /// reset the target (target is set to null)
        /// </summary>
        public void ResetTarget()
        {
            targetFollow = null;
        }

        /// <summary>
        /// calculate distance to ground
        /// </summary>
        /// <returns></returns>
		private float DistanceToGround()
		{
            Ray ray = new Ray(m_Transform.position, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, groundMask.value))
                return (hit.point - m_Transform.position).magnitude;

            return 0f;
        }

        #endregion
    }
}