using UnityEngine;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    [Header("Referências")]
    public GameObject tooltipObject; // O objeto marrom
    public TextMeshProUGUI txtName;
    public TextMeshProUGUI txtDescription;

    private RectTransform tooltipRT;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        if (tooltipObject != null)
            tooltipRT = tooltipObject.GetComponent<RectTransform>();
    }

    private void Start() => Hide();

    public void Show(string name, string desc, Vector3 slotPosition)
    {
        if (tooltipObject == null) return;

        txtName.text = name;
        txtDescription.text = desc;
        tooltipObject.SetActive(true);

        // --- LÓGICA DE POSICIONAMENTO ---

        // Garantimos que o Pivot esteja no Topo-Centro (X:0.5, Y:1)
        tooltipRT.pivot = new Vector2(0.5f, 1f);

        // Pegamos a posição do Slot e descemos 60 pixels 
        // (50px para sair do centro do slot + 10px de margem)
        Vector3 finalPos = slotPosition;
        finalPos.y -= 60f;

        // Movemos o RectTransform usando a posição global de tela
        tooltipRT.position = finalPos;
    }

    public void Hide()
    {
        if (tooltipObject != null) tooltipObject.SetActive(false);
    }
}