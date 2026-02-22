using UnityEngine;

[CreateAssetMenu(fileName = "Nova Arma", menuName = "Items/Arma")]
public class DadosArma : DadosItem
{
    [Header("Configurações de Combate")]
    public TipoAtaque tipoDeDano; // Perfurante ou Cortante

    [Header("Configurações do Minigame")]
    public float velocidadeDoAro = 1.5f; // Para o QTE Perfurante
    public float limiteDeTinta = 100f;   // Para o Pincel Cortante
}