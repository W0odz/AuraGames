public enum QuestStateFilter
{
    Sempre,       // Aparece em qualquer estado (ou quando não há quest vinculada)
    NotStarted,   // Quest ainda não foi aceita
    Active,       // Quest em andamento
    Completed,    // Todos os objetivos cumpridos, aguardando entrega
    TurnedIn      // Quest já entregue
}
