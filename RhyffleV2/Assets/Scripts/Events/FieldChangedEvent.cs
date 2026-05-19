using System.Collections.Generic;

public struct FieldChangedEvent {
    public List<CardData> Field;   // read-only snapshot — DO NOT mutate from subscribers
}
