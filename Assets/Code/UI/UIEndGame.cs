using Infrastructure;
using Infrastructure.States;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIEndGame : MonoBehaviour
    {
        public Button playForwardBtn;
        public Button toMainMenuBtn;
        
        
        private GameStateMachine _game;
        

        private void OnEnable()
        {
            playForwardBtn.onClick.AddListener(ContinuePlay);
            toMainMenuBtn.onClick.AddListener(ToMainMenu);
        }

        private void OnDisable()
        {
            playForwardBtn.onClick.RemoveListener(ContinuePlay);
            toMainMenuBtn.onClick.RemoveListener(ToMainMenu);
        }
        
        
        public UIEndGame Construct(GameStateMachine game)
        {
            _game = game;
            return this;
        }

        public void Show()
        {
            gameObject.SetActive(true);
            playForwardBtn.interactable = true;
            toMainMenuBtn.interactable = true;
        }

        public void Hide() => gameObject.SetActive(false);
        
        private void ContinuePlay()
        {
            _game.Enter<GameplayState>();
            playForwardBtn.interactable = false;
        }

        private void ToMainMenu()
        {
            _game.Enter<MainMenuState>();
            toMainMenuBtn.interactable = false;
        }
    }
}