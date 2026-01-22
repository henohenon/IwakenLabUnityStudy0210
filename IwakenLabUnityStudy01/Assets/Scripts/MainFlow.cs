using UnityEngine;
using R3;

namespace IwakenLabUnityStudy
{
    public class MainFlow : MonoBehaviour
    {
        [SerializeField] private TutorialFlow tutorialFlow;

        void OnEnable()
        {
            tutorialFlow.FinishTutorial.Subscribe(_ =>
            {
                tutorialFlow.gameObject.SetActive(false);
            });
            tutorialFlow.gameObject.SetActive(true);
        }
    }
}
