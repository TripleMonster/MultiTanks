using UnityEngine;
using System.Collections;

public class CollisionDetecte : MonoBehaviour
{
    BoxCollider m_Collider;

	public float MovingForce;
	Vector3 StartPoint;
	Vector3 Origin;
	public int NoOfRays = 10;
	int i;
	RaycastHit HitInfo;
	float LengthOfRay, DistanceBetweenRays, DirectionFactor;
	float margin = 0.015f;
	Ray ray;

	void Start()
	{
        m_Collider = GetComponent<BoxCollider> ();
		//Length of the Ray is distance from center to edge  
		LengthOfRay = m_Collider.bounds.extents.y;
		//Initialize DirectionFactor for upward direction  
		DirectionFactor = Mathf.Sign(Vector3.up.y);
	}

	void Update()
	{
		// First ray origin point for this frame  
		StartPoint = new Vector3(m_Collider.bounds.min.x + margin, transform.position.y, transform.position.z);
		if (!IsCollidingVertically())
		{
			transform.Translate(Vector3.up * MovingForce * Time.deltaTime * DirectionFactor);
		}
	}

	bool IsCollidingVertically()
	{
		Origin = StartPoint;
        DistanceBetweenRays = (m_Collider.bounds.size.x - 2 * margin) / (NoOfRays - 1);
        for (i = 0; i < NoOfRays; i++)
		{
			// Ray to be casted.  
			ray = new Ray(Origin, Vector3.up * DirectionFactor);
			//Draw ray on screen to see visually. Remember visual length is not actual length.  
			Debug.DrawRay(Origin, Vector3.up * DirectionFactor, Color.yellow);
			if (Physics.Raycast(ray, out HitInfo, LengthOfRay))
			{
                //Debug.Log("Collided With " + HitInfo.collider.gameObject.name);
				// Negate the Directionfactor to reverse the moving direction of colliding cube(here cube2)  
				DirectionFactor = -DirectionFactor;
				return true;
			}
			Origin += new Vector3(DistanceBetweenRays, 0, 0);
		}
		return false;

	}
}
