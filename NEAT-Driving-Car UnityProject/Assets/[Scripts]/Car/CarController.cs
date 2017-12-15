using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
	[SerializeField] private bool isAI = true;
	[SerializeField] private List<WheelCollider> wheelColliders = null;
	[SerializeField] private List<Transform> wheelMeshes = null;

	[SerializeField] private float enginePower = 150f;
	[SerializeField] private float maxSteer = 45f;
	[SerializeField] private Vector3 COM = new Vector3(0, 1.2f, -1.04f);
	[SerializeField] private GameObject rearLights = null;

	[Header("Read-only:")]
	[SerializeField] private float power = 0;
	[SerializeField] private float brake = 0;
	[SerializeField] private float steer = 0;

	Rigidbody myRigidbody = null;
	public Rigidbody MyRigidbody { get { return myRigidbody ?? (myRigidbody = GetComponent<Rigidbody>()); } }
	
	void Awake()
	{
		GetComponent<Rigidbody>().centerOfMass = COM;
	}
	
	void FixedUpdate ()
	{
		if (!isAI)
			Move(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
	}

	public void Move(float vertAxis, float horizontalAxis)
	{
		power = vertAxis * enginePower * Time.fixedDeltaTime * 720;
		steer = horizontalAxis * maxSteer;

		Vector3 localVel = transform.InverseTransformDirection(MyRigidbody.velocity);

		if ((power < 0 && localVel.z > 1) || (power > 0 && localVel.z < -1))
			brake = MyRigidbody.mass * 3;
		else if (power == 0)
			brake = 60;
		else
			brake = 0;

		UpdateWheels();
	}

	private void UpdateWheels()
	{
		wheelColliders[0].steerAngle = steer;
		wheelColliders[1].steerAngle = steer;

		//wheel heights
		for (int i = 0; i < wheelColliders.Count; i++)
		{
			Vector3 pos;
			Quaternion quat;

			wheelColliders[i].GetWorldPose(out pos, out quat);
			wheelMeshes[i].position = pos;
			wheelMeshes[i].rotation = quat;
		}

		if (brake > 0)
		{
			if (brake > 100)
				rearLights.SetActive(true);

			wheelColliders[0].brakeTorque = brake;
			wheelColliders[1].brakeTorque = brake;
			wheelColliders[2].brakeTorque = brake;
			wheelColliders[3].brakeTorque = brake;
			wheelColliders[2].motorTorque = 0;
			wheelColliders[3].motorTorque = 0;
		}
		else
		{
			if (rearLights.activeSelf)
				rearLights.SetActive(false);

			wheelColliders[0].brakeTorque = 0;
			wheelColliders[1].brakeTorque = 0;
			wheelColliders[2].brakeTorque = 0;
			wheelColliders[3].brakeTorque = 0;
			wheelColliders[2].motorTorque = power;
			wheelColliders[3].motorTorque = power;
		}
	}
}
