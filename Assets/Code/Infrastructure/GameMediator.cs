using System;
using Environment;
using Infrastructure.Services.Path;
using Infrastructure.Services.Tick;
using Infrastructure.Services.Time;
using Infrastucture;
using Player;
using UI;
using Unity.VisualScripting;
using UnityEngine;
using Utils;

namespace Infrastructure
{
    public class GameMediator : MonoBehaviour
    {
        [Header("Infrastructure")] //
        [SerializeField]
        private UnityTickService tickService;

        [SerializeField] private RectTransform loadingCurtain;

        [Header("UI Assets")] //
        [SerializeField]
        private UIMainMenu uiMainMenuAsset;

        [SerializeField] private UIRoundView roundViewUIAsset;
        [SerializeField] private UIRaceCountdown raceCountdownUIAsset;
        [SerializeField] private UIEndGame endGameMenuAsset;

        [Header("Game params")] //
        [SerializeField]
        private float raceCountdownDuration = 3;

        [Header("Game Assets")] //
        [SerializeField]
        private PathRecorder pathRecorderAsset;

        [SerializeField] private Map mapAsset;
        [SerializeField] private PlayerCarController playerAsset;
        [SerializeField] private Ghost.Ghost ghostAsset;

        [Header("Debug")] //
        public bool autoPlay;

        private GameStateMachine _game;
        private Controls _controls;
        private ITimeService _timeService;
        private RoundService _roundService;
        private Countdown _raceCountdown;

        private UIMainMenu _uiMainMenu;
        private UIRoundView _roundViewUI;
        private UIRaceCountdown _raceCountdownUI;
        private UIEndGame _endGameMenu;

        private PathRecorder _pathRecorder;
        private Map _map;
        private PlayerCarController _player;
        private Ghost.Ghost _ghost;


        public GameMediator Construct(GameStateMachine game)
        {
            _game = game;
            _controls = new Controls();
            _timeService = new UnityTimeService();
            _roundService = new RoundService();

            _raceCountdown = new Countdown(raceCountdownDuration, _timeService);

            tickService.Add(_game);
            tickService.Add(_raceCountdown);

            _controls.Enable();
            return this;
        }


        public void ShowLoadingCurtain() => loadingCurtain.gameObject.SetActive(true);
        public void HideLoadingCurtain() => loadingCurtain.gameObject.SetActive(false);


        public async Awaitable SetupMainMenu() =>
            _uiMainMenu ??= (await Assets.Create(uiMainMenuAsset)).Construct(_game);

        public void ShowMainMenu() => _uiMainMenu.Show();
        public void HideMainMenu() => _uiMainMenu.Hide();


        public async Awaitable SetupEndGameMenu() =>
            _endGameMenu ??= (await Assets.Create(endGameMenuAsset)).Construct(_game);

        public void ShowEndGameMenu() => _endGameMenu.Show();
        public void HideEndGameMenu() => _endGameMenu.Hide();


        public async Awaitable SetupRoundCounterUI() =>
            _roundViewUI ??= (await Assets.Create(roundViewUIAsset)).Construct(_roundService);

        public void ShowRoundCounterUI() => _roundViewUI.gameObject.SetActive(true);
        public void HideRoundCounterUI() => _roundViewUI.gameObject.SetActive(false);


        public async Awaitable SetupRaceCountdownUI() => _raceCountdownUI ??=
            (await Assets.Create(raceCountdownUIAsset)).Construct(_raceCountdown);

        public void ShowRaceCountdownUI() => _raceCountdownUI.gameObject.SetActive(true);
        public void HideRaceCountdownUI() => _raceCountdownUI.gameObject.SetActive(false);


        public async Awaitable SetupPathRecorder() =>
            _pathRecorder ??= (await Assets.Create(pathRecorderAsset)).Construct(_timeService);


        public async Awaitable SetupMap() => _map ??= (await Assets.Create(mapAsset)).Construct(_game);


        public async Awaitable SetupPlayerCar()
        {
            if (!PlayerExist())
                _player = (await Assets.Create(playerAsset)).Construct(_controls);

            var car = _player.car;

            car.gameObject.SetActive(false);

            var carT = car.transform;
            carT.position = _map.spawnPoint.position;
            carT.rotation = _map.spawnPoint.rotation;

            car.carVelocity = Vector3.zero;

            car.gameObject.SetActive(true);
        }

        public void DestroyPlayerCar()
        {
            Destroy(_player.gameObject);
            _player = null;
        }

        public bool PlayerExist() => !_player.IsUnityNull();
        public void EnablePlayerInput() => _controls.Player.Enable();
        public void DisablePlayerInput() => _controls.Player.Disable();


        public async Awaitable SetupGhost()
        {
            if (!GhostExist())
                _ghost = await Assets.Create(ghostAsset);

            var ghostT = _ghost.car.transform;
            ghostT.position = _map.spawnPoint.position;
            ghostT.rotation = _map.spawnPoint.rotation;

            _ghost.SetPath(_pathRecorder.PrevRecordedSpline);
        }

        public void DestroyGhost()
        {
            Destroy(_ghost.gameObject);
            _ghost = null;
        }

        public bool GhostExist() => !_ghost.IsUnityNull();
        public void EnableGhost() => _ghost.gameObject.SetActive(true);
        public void DisableGhost() => _ghost.gameObject.SetActive(false);


        public bool HasRecordedPath() => _pathRecorder.HasRecord;
        public void RecordPlayer() => _pathRecorder.Record(_player.car.transform);
        public void SaveRecord() => _pathRecorder.SaveRecord();


        public void NextRound() => _roundService.NextRound();

        public void StartRaceCountdown() => _raceCountdown.Start();
        public void SubscribeOnRaceStart(Action action) => _raceCountdown.OnComplete += action;
        public void UnsubscribeOnRaceStart(Action action) => _raceCountdown.OnComplete -= action;
    }
}