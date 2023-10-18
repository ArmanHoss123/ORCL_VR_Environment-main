using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class CollisionState : MonoBehaviour
{
    [System.Serializable]
    public class CollisionEvent : UnityEvent<GameObject> {}
    public enum CollisionType
    {
        Friendly = 0,
        Enemy = 1,
        FriendlyAndEnemy = 2
    }
    public CollisionType collisionType;
    [SerializeField]
    public CollisionEvent collisionEvent;

    protected Collision lastCollision;
    public Collision LastCollision {
        get {
            return lastCollision;
        }
    }

    protected virtual void OnCollisionEnter(Collision collision) 
    {
        CollisionState collisionObject = collision.gameObject.GetComponent<CollisionState>();
        if(collisionObject && collisionObject.collisionType != collisionType) {
            lastCollision = collision;
            ValidCollision(collision);
        }
    }

    protected virtual void ValidCollision(Collision collision)
    {
        collisionEvent.Invoke(collision.gameObject);
    }
}
