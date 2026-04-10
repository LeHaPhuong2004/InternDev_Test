using UnityEngine;
using UnityEngine.UI;

public class UIPanelWin : MonoBehaviour, IMenu
{
    [SerializeField] private Button btnBack;
    private UIMainManager m_mngr;

    private void Awake() => btnBack.onClick.AddListener(() => m_mngr.ShowMainMenu());

    public void Setup(UIMainManager mngr) => m_mngr = mngr;
    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}