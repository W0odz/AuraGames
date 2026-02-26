using UnityEngine;

public class PlayerBodyParts : MonoBehaviour
{
    [Tooltip("Definições das partes do corpo (ScriptableObjects) que podem ser atingidas.")]
    public BodyPartDefinition[] parts;

    public BodyPartDefinition Get(BodyPartType type)
    {
        if (parts == null) return null;

        foreach (var p in parts)
        {
            if (p != null && p.part == type)
                return p;
        }

        return null;
    }
}