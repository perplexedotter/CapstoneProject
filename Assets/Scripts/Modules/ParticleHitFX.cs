using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ParticleHitFX : MonoBehaviour {


    [SerializeField] GameObject hitFx;
    [SerializeField] Transform spawnParent;
    public ParticleSystem part;
    public List<ParticleCollisionEvent> collisionEvents;

    // Use this for initialization
    void Start()
    {
        part = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnParticleCollision(GameObject other)
    {
        part.GetCollisionEvents(other, collisionEvents);
        foreach (var c in collisionEvents)
        {
            Instantiate(hitFx, c.intersection, Quaternion.identity).transform.parent = spawnParent;
        }
    }
}
