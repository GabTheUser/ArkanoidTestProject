using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace MiniIT.ARKANOID
{
    public enum GameState
    {
        WaitingToStart = 0,
        Playing = 1,
        Win = 2,
        Lose = 3,
    }

    public class GameManager : MonoBehaviour
    {
        [Header("Ссылки на объекты")]
        [SerializeField] private BallController ball = null;

        [Header("Уровни")]
        [Tooltip("Корневые объекты уровней: Level1 Level2 Level3 и тп")]
        [SerializeField] private GameObject[] levels = null;

        [Tooltip("Префабы кирпичей (4 вида) Из них случайно выбираем для спавна")]
        [SerializeField] private Brick[] brickPrefabs = null;

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI inGameScoreText = null;
        [SerializeField] private TextMeshProUGUI endScoreText = null;
        [SerializeField] private GameObject startPanel = null;
        [SerializeField] private GameObject endPanel = null;
        [SerializeField] private TextMeshProUGUI endStateText = null;
        [SerializeField] private Color winColor = Color.white;
        [SerializeField] private Color loseColor = Color.white;

        [Header("End Screen Buttons")]
        [SerializeField] private GameObject restartButton = null;
        [SerializeField] private GameObject nextLevelButton = null;
        [SerializeField] private AudioClip winSound = null;
        [SerializeField] private AudioClip loseSound = null;

        private static int totalScore = 0;

        private int remainingBricks = 0;
        private GameState currentState = GameState.WaitingToStart;

        private List<Brick> spawnedBricks = null;

        private int currentLevelIndex = 0;
        private static int nextLevelIndex = 0;

        public GameState CurrentState
        {
            get { return currentState; }
        }

        private void Awake()
        {
            currentState = GameState.WaitingToStart;

            if (ball == null)
            {
                ball = FindFirstObjectByType<BallController>();
            }

            if (spawnedBricks == null)
            {
                spawnedBricks = new List<Brick>();
            }
        }

        private void Start()
        {
            SetupLevel();

            UpdateScoreUI();

            ShowStartPanel(true);
            ShowEndPanel(false);
        }

        private void OnDestroy()
        {
            if (spawnedBricks == null)
            {
                return;
            }

            for (int i = 0; i < spawnedBricks.Count; i++)
            {
                if (spawnedBricks[i] != null)
                {
                    spawnedBricks[i].BrickDestroyed -= OnBrickDestroyed;
                }
            }
        }

        private void SetupLevel()
        {
            if (levels == null || levels.Length == 0)
            {
                Debug.LogError("[GameManager] Не заданы уровни в инспекторе.");
                return;
            }

            if (nextLevelIndex >= levels.Length)
            {
                nextLevelIndex = 0;
            }

            currentLevelIndex = nextLevelIndex;

            for (int i = 0; i < levels.Length; i++)
            {
                if (levels[i] != null)
                {
                    levels[i].SetActive(i == currentLevelIndex);
                }
            }

            remainingBricks = 0;

            if (spawnedBricks == null)
            {
                spawnedBricks = new List<Brick>();
            }

            spawnedBricks.Clear();

            SpawnBricksForCurrentLevel();
        }

        private void SpawnBricksForCurrentLevel()
        {
            GameObject levelRoot = levels[currentLevelIndex];

            if (levelRoot == null)
            {
                Debug.LogError("[GameManager] Уровень с индексом " + currentLevelIndex + " не задан.");
                return;
            }

            Transform spawnsRoot = levelRoot.transform.Find("Spawns");

            if (spawnsRoot == null)
            {
                Debug.LogError("[GameManager] В уровне " + levelRoot.name + " не найден объект 'Spawns'.");
                return;
            }

            if (brickPrefabs == null || brickPrefabs.Length == 0)
            {
                Debug.LogError("[GameManager] Не заданы префабы кирпичей.");
                return;
            }

            int spawnCount = spawnsRoot.childCount;

            for (int i = 0; i < spawnCount; i++)
            {
                Transform spawnPoint = spawnsRoot.GetChild(i);

                Brick brickPrefab = GetRandomBrickPrefab();
                if (brickPrefab == null)
                {
                    continue;
                }

                Brick brickInstance = Instantiate(
                    brickPrefab,
                    spawnPoint.position,
                    Quaternion.identity
                );

                brickInstance.BrickDestroyed += OnBrickDestroyed;
                spawnedBricks.Add(brickInstance);
            }

            remainingBricks = spawnedBricks.Count;
            Debug.Log("[GameManager] Спавнено кирпичей: " + remainingBricks);
        }

        private Brick GetRandomBrickPrefab()
        {
            if (brickPrefabs == null || brickPrefabs.Length == 0)
            {
                return null;
            }

            int index = Random.Range(0, brickPrefabs.Length);
            return brickPrefabs[index];
        }

        public void StartGame()
        {
            if (currentState != GameState.WaitingToStart)
            {
                return;
            }

            currentState = GameState.Playing;

            ShowStartPanel(false);
            ShowEndPanel(false);
        }

        private void OnBrickDestroyed(Brick brick)
        {
            totalScore += brick.ScoreValue;
            remainingBricks--;

            UpdateScoreUI();

            if (currentState == GameState.Playing && remainingBricks <= 0)
            {
                HandleWin();
            }
        }

        public void HandleBallLost()
        {
            if (currentState != GameState.Playing)
            {
                return;
            }

            HandleLose();
        }

        private void HandleWin()
        {
            currentState = GameState.Win;

            nextLevelIndex = currentLevelIndex + 1;
            if (nextLevelIndex >= levels.Length)
            {
                nextLevelIndex = 0;
            }

            ShowEndPanel(true);
            if (endStateText != null)
            {
                endStateText.color = winColor;
            }

            SetEndStateText("Победа");

            if (winSound != null)
            {
                AudioSource.PlayClipAtPoint(winSound, transform.position, 1f);
            }

            if (restartButton != null)
            {
                restartButton.SetActive(false);
            }

            if (nextLevelButton != null)
            {
                nextLevelButton.SetActive(true);
            }
        }

        private void HandleLose()
        {
            currentState = GameState.Lose;

            nextLevelIndex = currentLevelIndex;

            ShowEndPanel(true);
            if (endStateText != null)
            {
                endStateText.color = loseColor;
            }

            SetEndStateText("Поражение");

            if (loseSound != null)
            {
                AudioSource.PlayClipAtPoint(loseSound, transform.position, 1f);
            }

            if (restartButton != null)
            {
                restartButton.SetActive(true);
            }

            if (nextLevelButton != null)
            {
                nextLevelButton.SetActive(false);
            }
        }

        public void RestartCurrentLevel()
        {
            if (spawnedBricks != null)
            {
                for (int i = 0; i < spawnedBricks.Count; i++)
                {
                    Brick brick = spawnedBricks[i];

                    if (brick != null)
                    {
                        brick.BrickDestroyed -= OnBrickDestroyed;
                        Destroy(brick.gameObject);
                    }
                }

                spawnedBricks.Clear();
            }

            remainingBricks = 0;

            if (ball != null)
            {
                ball.ResetBall();
            }

            currentState = GameState.WaitingToStart;

            ShowEndPanel(false);
            ShowStartPanel(true);

            SpawnBricksForCurrentLevel();
        }

        public void LoadNextLevel()
        {
            nextLevelIndex = currentLevelIndex + 1;
            if (nextLevelIndex >= levels.Length)
            {
                nextLevelIndex = 0;
            }

            ReloadScene();
        }

        private void ReloadScene()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.buildIndex);
        }

        private void UpdateScoreUI()
        {
            if (inGameScoreText != null)
            {
                inGameScoreText.text = totalScore.ToString();
            }

            if (endScoreText != null)
            {
                endScoreText.text = totalScore.ToString();
            }
        }

        private void ShowStartPanel(bool show)
        {
            if (startPanel != null)
            {
                startPanel.SetActive(show);
            }
        }

        private void ShowEndPanel(bool show)
        {
            if (endPanel != null)
            {
                endPanel.SetActive(show);
            }
        }

        private void SetEndStateText(string message)
        {
            if (endStateText != null)
            {
                endStateText.text = message;
            }
        }
    }
}
