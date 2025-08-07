using Infrastructure.Services.Path;
using TMPro;
using UnityEngine;

namespace UI
{
    public class UIRoundView : MonoBehaviour
    {
        public TextMeshProUGUI textMesh;
        public string format;
        
        private RoundService _roundService;


        public UIRoundView Construct(RoundService roundService)
        {
            _roundService = roundService;
            _roundService.OnRoundChange += ChangeView;
            return this;
        }

        private void OnDestroy()
        {
            _roundService.OnRoundChange -= ChangeView;
        }

        
        private void ChangeView(int round)
        {
            textMesh.text = string.Format(format, round);
        }
    }
}