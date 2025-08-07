using Infrastructure;
using Infrastructure.States;
using UnityEngine;
using UnityEngine.UI;

namespace Infrastucture
{
    public class UIMainMenu : MonoBehaviour
    {
        public Button playBtn;
        public Button exitBtn;
        
        
        private GameStateMachine _game;
        

        private void OnEnable()
        {
            playBtn.onClick.AddListener(StartGame);
            exitBtn.onClick.AddListener(Quit);
        }

        private void OnDisable()
        {
            playBtn.onClick.RemoveListener(StartGame);
            exitBtn.onClick.RemoveListener(Quit);
        }
        
        
        public UIMainMenu Construct(GameStateMachine game)
        {
            _game = game;
            return this;
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
        
        private void StartGame() => _game.Enter<GameplayState>();
        private static void Quit() => Application.Quit();
    }
}