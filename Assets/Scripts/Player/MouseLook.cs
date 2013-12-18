using UnityEngine;
using System.Collections;
[AddComponentMenu("Camera-Control/Mouse Look")]
public class MouseLook : MonoBehaviour {
	
	public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
	public RotationAxes axes = RotationAxes.MouseXAndY;
	public float sensitivityX = 15.0f;
	public float sensitivityY = 15.0f;
	
	public float minimumX = -360.0f;
	public float maximumX = 360.0f;
	
	public float minimumY = -60.0f;
	public float maximumY = 60.0f;

	public Camera mainCamera;

	public GameObject steveHead;
	public GameObject steveBody;
	
	float rotationX = 0.0f;
	float rotationY = 0.0f;
	
	Quaternion originalRotation;

	void Update ()
	{
		if(gameObject.GetComponent<MouseClick>().getWorld().gameObject.GetComponent<PauseMenu>().isPaused()) {
			return;
		}
		if (axes == RotationAxes.MouseXAndY)
		{
			// Read the mouse input axis
			rotationX += Input.GetAxis("Mouse X") * sensitivityX;
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			
			rotationX = ClampAngle (rotationX, minimumX, maximumX);
			rotationY = ClampAngle (rotationY, minimumY, maximumY);
			
			Quaternion xQuaternion = Quaternion.AngleAxis (rotationX, Vector3.up);
			Quaternion yQuaternion = Quaternion.AngleAxis (rotationY, -Vector3.right);
			
			mainCamera.transform.localRotation = originalRotation * yQuaternion;
			steveHead.transform.localRotation = originalRotation * yQuaternion;

			transform.localRotation = originalRotation * xQuaternion;
		}
		else if (axes == RotationAxes.MouseX)
		{
			rotationX += Input.GetAxis("Mouse X") * sensitivityX;
			rotationX = ClampAngle (rotationX, minimumX, maximumX);
			
			Quaternion xQuaternion = Quaternion.AngleAxis (rotationX, Vector3.up);
			transform.localRotation = originalRotation * xQuaternion;
		}
		else
		{
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			rotationY = ClampAngle (rotationY, minimumY, maximumY);
			
			Quaternion yQuaternion = Quaternion.AngleAxis (-rotationY, Vector3.right);
			mainCamera.transform.localRotation = originalRotation * yQuaternion;
			steveHead.transform.localRotation = originalRotation * yQuaternion;
		}
	}
	
	void Start ()
	{
		// Make the rigid body not change rotation
		if (rigidbody)
			rigidbody.freezeRotation = true;
		originalRotation = transform.localRotation;
	}
	
	public static float ClampAngle (float angle, float min, float max)
	{
		if (angle < -360.0f)
			angle += 360.0f;
		if (angle > 360.0f)
			angle -= 360.0f;
		return Mathf.Clamp (angle, min, max);
	}
}
