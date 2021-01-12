namespace Common
{
    using UnityEngine;
    using System.Collections.Generic;
    using HuaweiARUnitySDK;
    using System.Collections;
    using System;
    using Common;
    public class TrackingController : MonoBehaviour
    {
        [Tooltip("plane prefabs")]
        public GameObject planePrefabs;

        private List<ARPlane> newPlanes = new List<ARPlane>();

        public void Update()
        {
            _DrawPlane();
        }

        private void _DrawPlane()
        {
            newPlanes.Clear();
            ARFrame.GetTrackables<ARPlane>(newPlanes, ARTrackableQueryFilter.NEW);
            for (int i = 0; i < newPlanes.Count; i++)
            {
                GameObject planeObject = Instantiate(planePrefabs, Vector3.zero, Quaternion.identity, transform);
                planeObject.GetComponent<TrackedPlaneVisualizer>().Initialize(newPlanes[i]);
            }
        }
    }
}
