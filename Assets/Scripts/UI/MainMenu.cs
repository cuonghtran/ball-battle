using System.Collections;
using UnityEngine;

namespace MainGame
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _playPanelCG;
        [SerializeField] private CanvasGroup _modesPanelCG;
        [SerializeField] private GameObject _arModeButton;

        // Start is called before the first frame update
        IEnumerator Start()
        {
            CheckRunOnMobileDevices();
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        }

        public void OnPlayButton_Click()
        {
            LeanTween.alphaCanvas(_playPanelCG, 0, 0.15f).setOnComplete(DisplayModesPanel);
            _playPanelCG.blocksRaycasts = false;
        }

        void DisplayModesPanel()
        {
            LeanTween.alphaCanvas(_modesPanelCG, 1, 0.15f).setOnComplete(() => _modesPanelCG.blocksRaycasts = true);
        }

        public void OnPveButton_Click()
        {
            GameManager.gameMode = GameMode.PVE;
            ChangeScene("GameScene");
        }

        public void OnPvpButton_Click()
        {
            GameManager.gameMode = GameMode.PVP;
            ChangeScene("GameScene");
        }

        void ChangeScene(string sceneName)
        {
            SceneController.Instance.FadeAndLoadScene(ConstantsList.Scenes[sceneName]);
        }

        #region AR button

        private void CheckRunOnMobileDevices()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
                _arModeButton.SetActive(true);
            else
                _arModeButton.SetActive(false);
        }

        public void OnArButton_Click()
        {
            ChangeScene("ARScene");
        }

        #endregion
    }

}