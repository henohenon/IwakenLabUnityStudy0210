using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace IwakenLabUnityStudy
{
    public class MainFlow : MonoBehaviour
    {
        [SerializeField] private TutorialFlow tutorialFlow;
        private CancellationTokenSource _tokenSource;
        
        void OnEnable()
        {
            Debug.Log("TutorialFlow Enable");
            _tokenSource = new CancellationTokenSource();
            StartFlow(_tokenSource.Token).Forget();
        }

        private async UniTaskVoid StartFlow(CancellationToken token)
        {
            tutorialFlow.gameObject.SetActive(true);
            
            await tutorialFlow.ExecTutorial(token);

            tutorialFlow.gameObject.SetActive(false);
        }
        
        private void OnDisable()
        {
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
        }
    }
}
