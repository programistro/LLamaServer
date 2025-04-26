using System.Collections.Concurrent;
using System.Text;
using LLama.Common;

namespace LLama.WebAPI.Services;

// Модель пользовательской сессии
public class UserSession
{
    public string ConnectionId { get; set; }
    public ChatHistory History { get; set; } = new();
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
}

// Репозиторий для работы с сессиями
public interface ISessionRepository
{
    UserSession GetOrCreateSession(string connectionId);
    void UpdateSession(string connectionId, UserSession session);
    void RemoveSession(string connectionId);
}

// In-memory реализация (для примера)
public class MemorySessionRepository : ISessionRepository
{
    private readonly ConcurrentDictionary<string, UserSession> _sessions = new();

    public UserSession GetOrCreateSession(string connectionId)
    {
        return _sessions.GetOrAdd(connectionId, id => new UserSession { ConnectionId = id });
    }

    public void UpdateSession(string connectionId, UserSession session)
    {
        _sessions.AddOrUpdate(connectionId, session, (id, old) => session);
    }

    public void RemoveSession(string connectionId)
    {
        _sessions.TryRemove(connectionId, out _);
    }
}
