using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace MainGame
{
    [RequireComponent(typeof(ARPlaneManager))]
    public class PlaneVisualizerToggle : MonoBehaviour
    {
        private ARPlaneManager _planeManager;

        private void OnEnable()
        {
            _planeManager = GetComponent<ARPlaneManager>();
        }

        public void TogglePlaneDetection()
        {
            _planeManager.enabled = !_planeManager.enabled;
            string toggleButtonMessage = string.Empty;

            if (_planeManager.enabled)
            {
                toggleButtonMessage = "Disable AR Plane Visualizer";
                SetAllPlanesActive(true);
            }
            else
            {
                toggleButtonMessage = "Enable AR Plane Visualizer";
                SetAllPlanesActive(false);
            }
            UiArManager.OnTogglePlaneVisualizer(toggleButtonMessage);
        }

        private void SetAllPlanesActive(bool value)
        {
            foreach (var plane in _planeManager.trackables)
                plane.gameObject.SetActive(value);
        }
    }
}
