using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BreakIntoShards : MonoBehaviour
{
    [SerializeField] string[] CollisionLayersToBreakOn;
    [SerializeField] float ForceToBreak;
    [SerializeField] float ExplosionForce = 100f;
    [SerializeField] float ExplosionRadius = 100f;
    [SerializeField] Transform ShardsRoot;

    int[] _collisionLayersToBreakOn;
    private void Awake()
    {
        _collisionLayersToBreakOn = CollisionLayersToBreakOn.Select(LayerMask.NameToLayer).ToArray();
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (_collisionLayersToBreakOn.Contains(collision.gameObject.layer))
        {
            var collisionMagnitude = collision.impulse.magnitude;
            //Debug.Log($"Impact force: {collisionMagnitude}");
            if (collisionMagnitude >= ForceToBreak)
            {
                Debug.Log($"{name} breaks into shards!");
                var explosionForce = ExplosionForce * collisionMagnitude;
                ShardsRoot.parent = transform.parent;
                ShardsRoot.gameObject.SetActive(true);
                foreach(var shard in ShardsRoot.GetComponentsInChildren<Rigidbody>())
                {
                    shard.AddExplosionForce(explosionForce, collision.GetContact(0).point, ExplosionRadius,0f, ForceMode.VelocityChange);
                }
                Destroy(gameObject);
            }
        }

    }
}
