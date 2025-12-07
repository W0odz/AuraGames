using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    // Informações do Jogador
    public string playerName;
    public int playerLevel;
    public int currentXP;
    public int xpToNextLevel;

    // Recursos Atuais
    public int currentHP;
    public int currentMP;

    // Stats Base
    public int maxHP;
    public int maxMP;
    public int strength;
    public int speed;
    public int resistance;
    public int will;
    public int knowledge;
    public int luck;

    // Progresso do Jogo
    public List<string> defeatedEnemyIDs;

    public List<string> collectedItemIDs;

    // Dados de posição
    public string sceneName; // O nome da cena (Ex: "ExplorationScene")
    public float posX;
    public float posY;
    public float posZ;

    // O Construtor (valores padrão para um Jogo Novo)
    public GameData()
    {
        this.playerName = "Herói";
        this.playerLevel = 1;
        this.currentXP = 0;
        this.xpToNextLevel = 100;

        this.maxHP = 100;
        this.maxMP = 50;
        this.currentHP = 100; // Garanta que nasce cheio
        this.currentMP = 50;

        this.strength = 20;   // Aumente conforme necessário
        this.speed = 0;
        this.resistance = 5;
        this.will = 20;
        this.knowledge = 5;
        this.luck = 10;

        this.defeatedEnemyIDs = new List<string>();

        this.collectedItemIDs = new List<string>();
    }
}