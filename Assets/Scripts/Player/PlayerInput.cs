using UnityEngine;

namespace MainGame
{
    public class PlayerInput : MonoBehaviour
    {
        public Ray? GetRayFromInput()
        {
            if (ResolveMenu.gameIsPaused)
                return null;

            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                if (Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);
                    if (touch.phase == TouchPhase.Began)
                    {
                        Ray ray = Camera.main.ScreenPointToRay(touch.position);
                        return ray;
                    }
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                return ray;
            }

            return null;
        }
    }
}
