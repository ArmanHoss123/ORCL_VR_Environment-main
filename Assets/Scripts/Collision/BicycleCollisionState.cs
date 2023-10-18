using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BicycleCollisionState : CollisionState
{
   protected override void ValidCollision(Collision collision) 
   {
       base.ValidCollision(collision);
       //Show die/restart or just do it in the event. Maybe make bike go cray.
   }
}
