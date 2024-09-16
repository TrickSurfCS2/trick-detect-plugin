using Microsoft.Extensions.Logging;

namespace TrickDetect.Managers;

public class EventManager
{
    private readonly Dictionary<Type, List<Action<object>>> _syncSubscribers = new();

    public void Subscribe<T>(Action<T> handler) where T : class
    {
        var type = typeof(T);
        if (!_syncSubscribers.ContainsKey(type))
        {
            _syncSubscribers[type] = new List<Action<object>>();
        }
        _syncSubscribers[type].Add(e =>
        {
            if (e is T typedEvent)
            {
                handler(typedEvent);
            }
            else
            {
                TrickDetect._logger?.LogWarning($"Event {e.GetType()} cant cast to {type}");
            }
        });
    }

    public void Publish<T>(T eventToPublish) where T : class
    {
        if (eventToPublish == null)
        {
            TrickDetect._logger?.LogWarning("Event cant publish nothing.");
            return;
        }

        try
        {
            var type = typeof(T);
            if (_syncSubscribers.TryGetValue(type, out var handlers))
            {
                foreach (var handler in handlers)
                {
                    handler(eventToPublish);
                }
            }
        }
        catch (Exception ex)
        {
            TrickDetect._logger?.LogCritical(ex, "Event publish error.");
        }
    }
}
