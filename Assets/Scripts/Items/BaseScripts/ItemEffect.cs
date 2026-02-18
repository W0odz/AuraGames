using UnityEngine;

// Esse script permite criar arquivos de "Efeito" no seu projeto
public abstract class ItemEffect : ScriptableObject
{
    // Toda vez que um item for usado, ele chamará essa função
    public abstract void Execute(GameObject player);
}