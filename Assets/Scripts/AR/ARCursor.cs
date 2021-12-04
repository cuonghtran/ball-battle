using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace MainGame
{
    public class ARCursor : MonoBehaviour
    {
        public GameObject objectToPlace;
        public ARRaycastManager raycastManager;
        public Transform arOrigin;

        private GameObject gameBoard;
        private Camera _mainCamera;
        private bool wasSpawned = false;
        private float touchCount = 0;
        private float zoomSpeed = 0.01f;

        // Start is called before the first frame update
        void Start()
        {
            _mainCamera = Camera.main;
        }

        // Update is called once per frame
        void Update()
        {
            UpdateCursor();
            SpawnGameBoard();

            PinchToZoom();
            SwipeLeftAndRight();
        }

        private void UpdateCursor()
        {
            Vector2 screenPosition = Camera.main.ViewportToScreenPoint(new Vector2(0.5f, 0.5f));
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            raycastManager.Raycast(screenPosition, hits, TrackableType.Planes);

            if (hits.Count > 0)
            {
                var cameraForward = _mainCamera.transform.forward;
                var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
                transform.SetPositionAndRotation(hits[0].pose.position, Quaternion.LookRotation(cameraBearing));
            }
        }

        private void SpawnGameBoard()
        {
            if (wasSpawned) return;

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                gameBoard = Instantiate(objectToPlace, transform.position, transform.rotation);
                UiArManager.OnGameboardSpawned?.Invoke();
                wasSpawned = true;
            }
        }

        private void PinchToZoom()
        {
            touchCount = Input.touchCount;
            if (touchCount == 2)
            {
                Touch firstTouch = Input.GetTouch(0);
                Touch secondTouch = Input.GetTouch(1);

                Vector2 firstTouchPrevPos = firstTouch.position - firstTouch.deltaPosition;
                Vector2 secondTouchPrevPos = secondTouch.position - secondTouch.deltaPosition;

                float prevTouchDelta = (firstTouchPrevPos - secondTouchPrevPos).magnitude;
                float touchDelta = (firstTouch.position - secondTouch.position).magnitude;

                float zoomDelta = (firstTouch.deltaPosition - secondTouch.deltaPosition).magnitude * zoomSpeed;

                if (prevTouchDelta > touchDelta)
                {
                    gameBoard.transform.localScale = new Vector3(Mathf.Clamp(gameBoard.transform.localScale.x - zoomDelta, 0.8f, 1.2f),
                                                        Mathf.Clamp(gameBoard.transform.localScale.y - zoomDelta, 0.8f, 1.2f),
                                                        Mathf.Clamp(gameBoard.transform.localScale.z - zoomDelta, 0.8f, 1.2f));
                }
                if (prevTouchDelta < touchDelta)
                {
                    gameBoard.transform.localScale = new Vector3(Mathf.Clamp(gameBoard.transform.localScale.x + zoomDelta, 0.8f, 1.2f),
                                                        Mathf.Clamp(gameBoard.transform.localScale.y + zoomDelta, 0.8f, 1.2f),
                                                        Mathf.Clamp(gameBoard.transform.localScale.z + zoomDelta, 0.8f, 1.2f));
                }
            }
        }

        private void SwipeLeftAndRight()
        {
            if (gameBoard == null) return;

            touchCount = Input.touchCount;
            if (touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                Vector2 deltaPosition = Input.GetTouch(0).deltaPosition;

                // Handle left & right 
                if (deltaPosition.x > 0.01f || deltaPosition.x < -0.01f)
                {
                    gameBoard.transform.eulerAngles = new Vector3(gameBoard.transform.eulerAngles.x,
                        gameBoard.transform.eulerAngles.y + deltaPosition.y * 0.15f,
                        gameBoard.transform.eulerAngles.z);
                }

            }
        }
    }
}
