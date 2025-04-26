using System.Collections.Concurrent;
using System.Text;
using LLama.Abstractions;
using LLama.Common;
using LLama.Sampling;
using LLama.WebAPI.Services;
using Microsoft.AspNetCore.SignalR;

namespace LLama.WebApi.Hubs;

public class LlamaService : IDisposable
{
    private readonly LLamaWeights _model;
    private readonly ISessionRepository _sessionRepository;
    private readonly ConcurrentDictionary<string, LLamaContext> _contexts = new();

    public LlamaService(ISessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
        
        var parameters = new ModelParams("C:/Users/katana/Downloads/Llama-2-7b-chat-hf-finetune-q5_k_m-v1.0.gguf")
        {
            ContextSize = 2048,
            GpuLayerCount = 5
        };
        _model = LLamaWeights.LoadFromFile(parameters);
    }

    public async IAsyncEnumerable<string> GenerateResponse(string connectionId, string prompt)
    {
        var session = _sessionRepository.GetOrCreateSession(connectionId);
        
        // Создаем контекст для каждого подключения
        var context = _contexts.GetOrAdd(connectionId,
            id => _model.CreateContext(new ModelParams("C:/Users/katana/Downloads/Llama-2-7b-chat-hf-finetune-q5_k_m-v1.0.gguf")
                { ContextSize = 2048, GpuLayerCount = 5 }));
        
        var executor = new InteractiveExecutor(context);
        var chatSession = new ChatSession(executor);

        session.History.AddMessage(AuthorRole.User, prompt);
        
        var response = chatSession.ChatAsync(
            session.History, 
            new InferenceParams { AntiPrompts = ["User:"], TokensKeep = 20 }, 
            CancellationToken.None
        );

        var fullResponse = new StringBuilder();
        await foreach(var token in response)
        {
            fullResponse.Append(token);
            yield return token;
        }
        
        session.History.AddMessage(AuthorRole.Assistant, fullResponse.ToString());
        session.LastActivity = DateTime.UtcNow;
        _sessionRepository.UpdateSession(connectionId, session);
    }

    public void Dispose()
    {
        foreach (var context in _contexts.Values)
            context.Dispose();
        _model.Dispose();
    }
}

public class AiHub : Hub
{
    private readonly LlamaService _llama;
    private readonly ISessionRepository _sessionRepository;

    public AiHub(LlamaService llama, ISessionRepository sessionRepository)
    {
        _llama = llama;
        _sessionRepository = sessionRepository;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _sessionRepository.RemoveSession(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendPrompt(string prompt)
    {
        await foreach(var token in _llama.GenerateResponse(Context.ConnectionId, prompt))
        {
            await Clients.Caller.SendAsync("ReceiveToken", token);
        }
        
        await Clients.Caller.SendAsync("StreamComplete");
    }
}
