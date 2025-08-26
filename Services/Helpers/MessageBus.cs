namespace FileCloud.Desktop.Helpers
{
    public class MessageBus
    {
        private readonly Dictionary<Type, List<Delegate>> _subscribers = new();

        public void Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (!_subscribers.ContainsKey(type))
                _subscribers[type] = new List<Delegate>();

            _subscribers[type].Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (_subscribers.ContainsKey(type))
            {
                _subscribers[type].Remove(handler);
                if (_subscribers[type].Count == 0)
                    _subscribers.Remove(type);
            }
        }

        public async Task Publish<T>(T message)
        {
            var type = typeof(T);
            if (_subscribers.ContainsKey(type))
            {
                foreach (var handler in _subscribers[type])
                {
                    ((Action<T>)handler)?.Invoke(message);
                }
            }
        }
    }
}
