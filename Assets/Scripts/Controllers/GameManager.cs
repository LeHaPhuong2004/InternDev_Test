using DG.Tweening;
using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public event Action<eStateGame> StateChangedAction = delegate { };

    public enum eLevelMode { TIMER, MOVES }
    public enum eStateGame { SETUP, MAIN_MENU, GAME_STARTED, PAUSE, GAME_OVER, WIN }

    private eStateGame m_state;
    public eStateGame State
    {
        get => m_state;
        private set
        {
            m_state = value;
            StateChangedAction(m_state);
        }
    }

    private bool m_gameEnded = false;

    private GameSettings m_gameSettings;
    [SerializeField] private BoardController m_boardController;

    private UIMainManager m_uiMenu;
    private LevelCondition m_levelCondition;

    private void Awake()
    {
        State = eStateGame.SETUP;

        m_gameSettings = Resources.Load<GameSettings>(Constants.GAME_SETTINGS_PATH);

        m_uiMenu = FindObjectOfType<UIMainManager>();
        m_uiMenu.Setup(this);
    }

    void Start()
    {
        State = eStateGame.MAIN_MENU;
    }

    public void SetState(eStateGame state)
    {
        State = state;

        if (State == eStateGame.PAUSE)
            DOTween.PauseAll();
        else
            DOTween.PlayAll();
    }

    public void LoadLevel(eLevelMode mode)
    {
        m_gameEnded = false;

        m_boardController.StartGame(this, m_gameSettings);

        if (mode == eLevelMode.MOVES)
        {
            m_levelCondition = gameObject.AddComponent<LevelMoves>();
            m_levelCondition.Setup(
                m_gameSettings.LevelMoves,
                m_uiMenu.GetLevelConditionView(),
                m_boardController
            );
        }
        else
        {
            m_levelCondition = gameObject.AddComponent<LevelTime>();
            m_levelCondition.Setup(
                m_gameSettings.LevelTime,
                m_uiMenu.GetLevelConditionView(),
                this
            );
        }

        m_levelCondition.ConditionCompleteEvent += OnLevelFinished;

        State = eStateGame.GAME_STARTED;
    }

    private void OnLevelFinished()
    {
        if (m_gameEnded) return;

        m_gameEnded = true;
        SetState(eStateGame.GAME_OVER);
    }

    public void ClearLevel()
    {
        if (m_levelCondition != null)
        {
            Destroy(m_levelCondition);
        }

        if (m_boardController != null)
        {
            m_boardController.Clear();
        }
    }
}