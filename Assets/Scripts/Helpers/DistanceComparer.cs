using System.Collections;
using System;
using UnityEngine;

namespace MainGame
{
    public class DistanceComparer : IComparer
    {
        private Transform compareTransform;

        public DistanceComparer(Transform compTransform)
        {
            compareTransform = compTransform;
        }

        public int Compare(object x, object y)
        {
            Collider xCollider = x as Collider;
            Collider yCollider = y as Collider;

            float xDistance = (xCollider.transform.position - compareTransform.position).magnitude;
            float yDistance = (yCollider.transform.position - compareTransform.position).magnitude;

            return xDistance.CompareTo(yDistance);
        }
    }
}
