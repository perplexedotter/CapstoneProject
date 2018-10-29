using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeltSpawner : MonoBehaviour {

 //for creating asteroids
    [Header("Asteroid Properties")]
    [SerializeField] GameObject AsteroidPrefab;
    [SerializeField] int meteorDensity;
    [SerializeField] int seed;
    [SerializeField] float innerRadius;
    [SerializeField] float outerRadius;
    [SerializeField] float height;
    [SerializeField] bool rotatingClockwise;

    [Header("Asteroid Spawning")]
    [SerializeField] float minOrbitSpeed;
    [SerializeField] float maxOrbitSpeed;
	[SerializeField] float minRotationSpeed;
    [SerializeField] float maxRotationSpeed;

	private Vector3 localPosition;
	private Vector3 worldOffset;
	private Vector3 worldPosition;
	private float randomRadius;
	private float randomRadian;
	private float x;
	private float y;
	private float z;
	
	

	// Use this for initialization
	void Start () {
		//rand seed
		Random.InitState(seed);
		//create meteorsaround central body
		for (int i = 0; i < meteorDensity; i++)
		{
			//set randoms and set coords, calculate positions
			do
			{
				randomRadius = Random.Range(innerRadius, outerRadius);
				randomRadian = Random.Range(0, 2*Mathf.PI);
				
				y = Random.Range(-(height/2), (height/2));
				x = randomRadius * Mathf.Cos(randomRadian);
				z = randomRadius * Mathf.Sin(randomRadian);
				
			} while (float.IsNaN(z) && float.IsNaN(x));

			localPosition = new Vector3(x,y,z);
			worldOffset = transform.rotation * localPosition;
			worldPosition =  transform.position + worldOffset; 
			//create object with calculated position on a random rotation
			GameObject _smallAsteroid = Instantiate(AsteroidPrefab, worldPosition, Quaternion.Euler(Random.Range(0,360),Random.Range(0,360),Random.Range(0,360)));
			
			_smallAsteroid.AddComponent<BeltObject>().SetupBeltObject(Random.Range(minOrbitSpeed, maxOrbitSpeed), Random.Range(minRotationSpeed, maxRotationSpeed), gameObject, rotatingClockwise);
			_smallAsteroid.transform.SetParent(transform);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
