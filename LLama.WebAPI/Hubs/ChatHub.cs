using System.Text;
using LLama.Abstractions;
using LLama.Common;
using LLama.Sampling;
using Microsoft.AspNetCore.SignalR;

namespace LLama.WebApi.Hubs;

public class LlamaService : IDisposable
{
    private LLamaWeights _model;
    private LLamaContext _context;
    private ChatSession _session;
    private ChatHistory _history;

    public LlamaService()
    {
        var parameters = new ModelParams("C:/Users/katana/Downloads/llama-2-7b-guanaco-qlora.Q3_K_S.gguf")
        {
            ContextSize = 2048,
            GpuLayerCount = 5
        };
        
        _model = LLamaWeights.LoadFromFile(parameters);
        _context = _model.CreateContext(parameters);
        var executor = new InteractiveExecutor(_context);
        _session = new ChatSession(executor);
        _history = new ChatHistory();
    }

    public async IAsyncEnumerable<string> GenerateResponse(string prompt)
    {
        var inferenceParams = new InferenceParams
        {
            AntiPrompts = new List<string> { "User:" },
            TokensKeep = 20
        };

        _history.AddMessage(AuthorRole.User, prompt);
        
        var response = _session.ChatAsync(
            _history, 
            inferenceParams, 
            CancellationToken.None
        );

        await foreach(var token in response)
        {
            yield return token;
        }
        
        _history.AddMessage(AuthorRole.Assistant, string.Join("", response));
    }

    public void Dispose()
    {
        _session?.SaveSession("session.json");
        _context?.Dispose();
        _model?.Dispose();
    }
}


public class AiHub : Hub
{
    private readonly LlamaService _llama;

    public AiHub(LlamaService llama) => _llama = llama;

    public async Task SendPrompt(string prompt)
    {
        await foreach(var token in _llama.GenerateResponse(prompt))
        {
            await Clients.Caller.SendAsync("ReceiveToken", token);
        }
        
        await Clients.Caller.SendAsync("StreamComplete");
    }
}