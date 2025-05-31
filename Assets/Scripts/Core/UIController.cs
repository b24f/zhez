using UnityEngine;

namespace Core
{
    public class UIController : MonoBehaviour
    {
        public static UIController Current;

        [SerializeField] private GameObject HUDComponent;
        [SerializeField] private GameObject MenuComponent;
        [SerializeField] private GameObject ExamineComponent;
        [SerializeField] private GameObject GameOverComponent;
        private void Awake()
        {
            Current = this;
        }

        public void Reset()
        {
            MenuComponent.SetActive(false);
            HUDComponent.SetActive(false);
            ExamineComponent.SetActive(false);
            GameOverComponent.SetActive(false);
        }

        public void RenderMenu()
        {
            Reset();
            MenuComponent.SetActive(true);
        }

        public void RenderHUD()
        {
            Reset();
            HUDComponent.SetActive(true);
        }

        public void RenderExamine()
        {
            Reset();
            ExamineComponent.SetActive(true);
        }

        public void RenderGameOver()
        {
            Reset();
            GameOverComponent.SetActive(true);
        }
    }
}