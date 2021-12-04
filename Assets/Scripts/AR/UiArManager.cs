using System;
using UnityEngine;
using TMPro;
using UnityEngine.XR.ARFoundation;

namespace MainGame
{
    public class UiArManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] GameObject _normalCamera;
        [SerializeField] GameObject _arCamera;

        [Header("UI")]
        [SerializeField] GameObject _unsupportedPanel;
        [SerializeField] GameObject _supportedPanel;
        [SerializeField] GameObject _tapToPlaceButton;
        [SerializeField] TMP_Text _arPlaneVisualizerText;
        [SerializeField] TMP_Text _arModeText;

        public static Action<bool> OnArSessionStateChecked;
        public static Action OnGameboardSpawned;
        public static Action<string> OnTogglePlaneVisualizer;

        private void OnEnable()
        {
            OnArSessionStateChecked += ToggleSupportPanel;
            OnGameboardSpawned += TurnOffTapToPlaceButton;
            OnTogglePlaneVisualizer += TogglePlaneVisualizer;
        }

        private void OnDisable()
        {
            OnArSessionStateChecked -= ToggleSupportPanel;
            OnGameboardSpawned -= TurnOffTapToPlaceButton;
            OnTogglePlaneVisualizer -= TogglePlaneVisualizer;
        }

        public void ExitButton_Click()
        {
            SceneController.Instance.FadeAndLoadScene(ConstantsList.Scenes["OpeningScene"]);
        }

        public void ARModeButton_Click()
        {
            _normalCamera.SetActive(!_normalCamera.activeSelf);
            _arCamera.SetActive(!_arCamera.activeSelf);
            _arModeText.text = _arCamera.activeSelf ? "Turn off AR Mode" : "Turn on AR Mode";
        }

        private void ToggleSupportPanel (bool isSupportedDevice)
        {
            _supportedPanel.SetActive(isSupportedDevice);
            _unsupportedPanel.SetActive(!isSupportedDevice);
        }

        private void TurnOffTapToPlaceButton()
        {
            _tapToPlaceButton.SetActive(false);
        }

        private void TogglePlaneVisualizer(string buttonText)
        {
            _arPlaneVisualizerText.text = buttonText;
        }
    }
}
