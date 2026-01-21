using System;
using UnityEngine;
using R3;

namespace IwakenLabUnityStudy
{
    public class MainFlow : MonoBehaviour
    {
        [SerializeField] private TutorialFlow tutorialFlow;
        [SerializeField] private DrawFlow drawFlow;
        [SerializeField] private WaitFlow waitFlow;
        [SerializeField] private BattleFlow battleFlow;
        [SerializeField] private ResultFlow resultFlow;

        void OnEnable()
        {
            tutorialFlow.FinishTutorial.Subscribe(_ =>
            {
                tutorialFlow.gameObject.SetActive(false);
                drawFlow.gameObject.SetActive(true);
            });
            drawFlow.FinishDraw.Subscribe(data =>
            {
                drawFlow.gameObject.SetActive(false);
                waitFlow.Initialize();
            });
            waitFlow.FinishWait.Subscribe(_ =>
            {
                waitFlow.gameObject.SetActive(false);
                battleFlow.Initialize();
            });
            battleFlow.OnFinishBattle.Subscribe(isClear =>
            {
                battleFlow.gameObject.SetActive(false);
                resultFlow.Initialize(isClear);
            });

            drawFlow.gameObject.SetActive(false);
            battleFlow.gameObject.SetActive(false);
            resultFlow.gameObject.SetActive(false);
            tutorialFlow.gameObject.SetActive(true);
        }
    }
}