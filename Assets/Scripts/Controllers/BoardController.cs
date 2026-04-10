using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    [SerializeField] private Transform[] traySlots;
    [SerializeField] private int trayMax = 5;

    private List<Item> trayItems = new List<Item>();

    public bool IsBusy { get; private set; }

    private Board m_board;
    private GameManager m_gameManager;
    private Camera m_cam;
    private GameSettings m_gameSettings;
    private bool m_gameOver;

    public event Action OnMoveEvent = delegate { };

    private void Awake()
    {
        m_cam = Camera.main;
    }

    public void StartGame(GameManager gameManager, GameSettings gameSettings)
    {
        m_gameManager = gameManager;
        m_gameSettings = gameSettings;

        m_gameManager.StateChangedAction += OnGameStateChange;

        m_board = new Board(this.transform, gameSettings);

        Fill();
    }

    private void Fill()
    {
        m_board.Fill();
    }

    private void OnGameStateChange(GameManager.eStateGame state)
    {
        switch (state)
        {
            case GameManager.eStateGame.GAME_STARTED:
                IsBusy = false;
                break;
            case GameManager.eStateGame.PAUSE:
                IsBusy = true;
                break;
            case GameManager.eStateGame.GAME_OVER:
                m_gameOver = true;
                break;
        }
    }

    private void Update()
    {
        if (m_gameOver || IsBusy) return;

        if (Input.GetMouseButtonDown(0))
        {
            var hit = Physics2D.Raycast(
                m_cam.ScreenToWorldPoint(Input.mousePosition),
                Vector2.zero
            );

            if (hit.collider != null)
            {
                Cell cell = hit.collider.GetComponent<Cell>();

                if (cell != null && cell.Item != null)
                {
                    OnCellClicked(cell);
                }
            }
        }
    }

    private void OnCellClicked(Cell cell)
    {
        if (IsBusy) return;

        if (trayItems.Count == trayMax)
        {
            m_gameManager.SetState(GameManager.eStateGame.GAME_OVER);
            m_gameOver = true; 
            Debug.Log("LOSE");
            return;
        }

        IsBusy = true;

        Item item = cell.Item;
        cell.Free();

        trayItems.Add(item);
        OnMoveEvent?.Invoke();

        item.SetViewRoot(this.transform);

        int index = trayItems.Count - 1;
        Vector3 targetPos = traySlots[index].position;

        item.View.DOMove(targetPos, 0.3f).OnComplete(() =>
        {
            CheckMatchInTray();
            RearrangeTray();

            if (m_gameManager.State == GameManager.eStateGame.GAME_STARTED && IsBoardCleared())
            {
                
                m_gameManager.SetState(GameManager.eStateGame.WIN);
            }

            IsBusy = false;
        });
    }

    private void CheckMatchInTray()
    {
        List<Item> matched = new List<Item>();

        for (int i = 0; i < trayItems.Count; i++)
        {
            List<Item> group = new List<Item>();

            foreach (var other in trayItems)
            {
                if (trayItems[i].IsSameType(other))
                {
                    group.Add(other);
                }
            }

            if (group.Count >= 3)
            {
                matched = group.Take(3).ToList();
                break;
            }
        }

        foreach (var item in matched)
        {
            item.ExplodeView();
            trayItems.Remove(item);
        }
    }

    private void RearrangeTray()
    {
        for (int i = 0; i < trayItems.Count; i++)
        {
            trayItems[i].View.DOMove(traySlots[i].position, 0.2f);
        }
    }

    public bool IsBoardCleared()
    {
        return m_board.IsEmpty();
    }

    public void Clear()
    {
        IsBusy = false;
        m_gameOver = false;

        foreach (var item in trayItems)
        {
            if (item != null)
                item.Clear();
        }

        trayItems.Clear();

        foreach (Transform slot in traySlots)
        {
            for (int i = slot.childCount - 1; i >= 0; i--)
            {
                Destroy(slot.GetChild(i).gameObject);
            }
        }

        if (m_board != null)
        {
            m_board.Clear();
        }
    }
}