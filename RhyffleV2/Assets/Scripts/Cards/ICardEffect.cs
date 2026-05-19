public interface ICardEffect {
    string Id { get; }
    void OnAttach(CardSystem ctx);   // card subscribes to events via ctx
    void OnDetach(CardSystem ctx);   // card unsubscribes
}
