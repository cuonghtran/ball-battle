using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace MainGame
{
    public class ARModeManager : MonoBehaviour
    {
        [SerializeField] private ARSession m_Session;

        IEnumerator Start()
        {
            if ((ARSession.state == ARSessionState.None) ||
                (ARSession.state == ARSessionState.CheckingAvailability))
            {
                yield return ARSession.CheckAvailability();
            }

            if (ARSession.state == ARSessionState.Unsupported)
            {
                // Start some fallback experience for unsupported devices
                UiArManager.OnArSessionStateChecked?.Invoke(false);
            }
            else
            {
                // Start the AR session
                UiArManager.OnArSessionStateChecked?.Invoke(true);
                m_Session.enabled = true;
            }
        }
    }
}
