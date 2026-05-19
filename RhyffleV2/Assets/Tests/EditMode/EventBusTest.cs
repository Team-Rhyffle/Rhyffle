using System;
using NUnit.Framework;

public class EventBusTest {

    // ── Dummy event types used across tests ─────────────────────────────────

    struct EventA { public int Value; }
    struct EventB { public int Value; }

    // ── Tests ────────────────────────────────────────────────────────────────

    [Test]
    public void Subscribe_Publish_HandlerInvokedOnceWithCorrectPayload() {
        int received = 0;
        int receivedValue = -1;
        Action<EventA> handler = e => { received++; receivedValue = e.Value; };

        try {
            EventBus.Subscribe(handler);
            EventBus.Publish(new EventA { Value = 42 });

            Assert.AreEqual(1, received,       "Handler should be invoked exactly once");
            Assert.AreEqual(42, receivedValue, "Handler should receive the published payload");
        }
        finally {
            EventBus.Unsubscribe(handler);
        }
    }

    [Test]
    public void Unsubscribe_AfterUnsubscribe_HandlerNotInvoked() {
        int received = 0;
        Action<EventA> handler = e => received++;

        EventBus.Subscribe(handler);
        EventBus.Unsubscribe(handler);
        EventBus.Publish(new EventA { Value = 1 });

        Assert.AreEqual(0, received, "Unsubscribed handler must not be invoked");
    }

    [Test]
    public void MultipleSubscribers_BothInvokedOnSinglePublish() {
        int countA = 0;
        int countB = 0;
        Action<EventA> handlerA = e => countA++;
        Action<EventA> handlerB = e => countB++;

        try {
            EventBus.Subscribe(handlerA);
            EventBus.Subscribe(handlerB);
            EventBus.Publish(new EventA { Value = 0 });

            Assert.AreEqual(1, countA, "First handler should be invoked once");
            Assert.AreEqual(1, countB, "Second handler should be invoked once");
        }
        finally {
            EventBus.Unsubscribe(handlerA);
            EventBus.Unsubscribe(handlerB);
        }
    }

    [Test]
    public void TypeIsolation_EventBHandlerNotInvokedWhenEventAPublished() {
        int receivedB = 0;
        Action<EventB> handlerB = e => receivedB++;

        try {
            EventBus.Subscribe(handlerB);
            EventBus.Publish(new EventA { Value = 99 }); // publish A, not B

            Assert.AreEqual(0, receivedB, "EventB handler must not fire when EventA is published");
        }
        finally {
            EventBus.Unsubscribe(handlerB);
        }
    }
}
