using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MainGame
{
    public class SceneController : MonoBehaviour
    {
        public static SceneController Instance;

        [SerializeField] private GameObject sceneControllerCamera;
        [SerializeField] private CanvasGroup faderCanvasGroup;

        public string startingSceneName = ConstantsList.Scenes["OpeningScene"];
        private float fadeDuration = 0.3f;
        private bool isFading;


        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        // Start is called before the first frame update
        private IEnumerator Start()
        {
            // Start off game with black screen
            faderCanvasGroup.alpha = 1f;

            // Start first scene
            yield return StartCoroutine(FadeAndSwitchScenes(startingSceneName));

            StartCoroutine(Fade(0f));
        }

        public void FadeAndLoadScene(string sceneName)
        {
            if (!isFading)
                StartCoroutine(FadeAndSwitchScenes(sceneName));
        }

        private IEnumerator FadeAndSwitchScenes(string sceneName)
        {
            yield return StartCoroutine(Fade(1f));
            sceneControllerCamera.SetActive(true);

            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);

            yield return StartCoroutine(LoadSceneAndSetActive(sceneName));

            sceneControllerCamera.SetActive(false);
            yield return StartCoroutine(Fade(0f));

            if (sceneName == ConstantsList.Scenes["GameScene"])
                GameManager.StartMatch();
        }

        private IEnumerator LoadSceneAndSetActive(string sceneName)
        {
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            Scene newlyLoadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            SceneManager.SetActiveScene(newlyLoadedScene);
        }

        private IEnumerator Fade(float finalAlpha)
        {
            isFading = true;
            faderCanvasGroup.blocksRaycasts = true;

            float fadeSpeed = Mathf.Abs(faderCanvasGroup.alpha - finalAlpha) / fadeDuration;
            while (!Mathf.Approximately(faderCanvasGroup.alpha, finalAlpha))
            {
                faderCanvasGroup.alpha = Mathf.MoveTowards(faderCanvasGroup.alpha, finalAlpha, fadeSpeed * Time.deltaTime);
                yield return null;
            }

            isFading = false;
            faderCanvasGroup.blocksRaycasts = false;
        }

        public string GetActiveSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }
    }
}
