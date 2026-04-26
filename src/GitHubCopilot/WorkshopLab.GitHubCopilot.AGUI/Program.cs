using Microsoft.Agents.AI;
using Microsoft.Agents.AI.AGUI;
using Microsoft.Extensions.AI;

var serverUrl = Environment.GetEnvironmentVariable("AGUI_SERVER_URL") ?? "http://localhost:8888/ag-ui";

Console.WriteLine($"Connecting to AG-UI server at: {serverUrl}");
Console.WriteLine("Type a message, or :q / quit to exit.");

using HttpClient httpClient = new()
{
    Timeout = TimeSpan.FromSeconds(120)
};

AGUIChatClient chatClient = new(httpClient, serverUrl);
AIAgent agent = chatClient.AsAIAgent(
    name: "github-copilot-agui-client",
    description: "AG-UI client for the GitHub Copilot readiness coach");

AgentSession session = await agent.CreateSessionAsync();
List<ChatMessage> messages =
[
    new(ChatRole.System, "You are connected to the GitHub Copilot AG-UI readiness coach.")
];

while (true)
{
    Console.Write("\nUser> ");
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
    {
        Console.WriteLine("Please enter a prompt.");
        continue;
    }

    if (input.Equals(":q", StringComparison.OrdinalIgnoreCase)
        || input.Equals("quit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    messages.Add(new ChatMessage(ChatRole.User, input));

    Console.Write("Assistant> ");
    var assistantText = string.Empty;

    await foreach (AgentResponseUpdate update in agent.RunStreamingAsync(messages, session))
    {
        if (!string.IsNullOrEmpty(update.Text))
        {
            Console.Write(update.Text);
            assistantText += update.Text;
        }
    }

    Console.WriteLine();

    if (!string.IsNullOrWhiteSpace(assistantText))
    {
        messages.Add(new ChatMessage(ChatRole.Assistant, assistantText));
    }
}
