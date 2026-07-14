using Microsoft.Agents.AI.Workflows;
using Microsoft.AspNetCore.WebUtilities;
using Parley.Configuration.Attributes;
using Parley.Workflows.Links;
using Parley.Workflows.Nodes;
using Parley.Workflows.State;
using System.Text.Json.Nodes;

namespace Application.Parley.Workflow.Nodes.CatFacts;

[ParleyNode]
[SendsMessage(typeof(ParleyLink))]
public class CatFactsNode : ParleyNode<ParleyLink>
{
    private const string BaseRequestUrl = "https://catfact.ninja/fact";
    private const string MaxLengthParam = "max_length";

    private readonly IHttpClientFactory _httpClientFactory;

    public CatFactsNode(ParleyNodeContext context,
                        IWorkflowStateManager workflowStateManager,
                        IHttpClientFactory httpClientFactory)
    : base(nameof(CatFactsNode), context, workflowStateManager)
    {
        _httpClientFactory = httpClientFactory;
    }

    public override string DialogType => nameof(CatFactsNode);

    public override async ValueTask HandleAsync(ParleyLink message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = GetNodeOptions<CatFactsNodeOptions>();

            var response = await SendRequest(options, context, cancellationToken);

            await ProcessResponse(response, options, context, cancellationToken);
        }
        catch
        {
            await context.SendMessageAsync(new ParleyLink((Guid)NodeConfig.SecondaryTransitionNode!), cancellationToken);
            return;
        }

        await context.SendMessageAsync(new ParleyLink(NodeConfig.PrimaryTransitionNode), cancellationToken);
    }

    private async Task ProcessResponse(HttpResponseMessage response,
                                       CatFactsNodeOptions options,
                                       IWorkflowContext context,
                                       CancellationToken cancellationToken)
    {
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        var root = JsonNode.Parse(payload);

        try
        {
            if (root?["fact"] is JsonValue value && value.TryGetValue<string>(out var fact))
            {
                var workflowVariable = await WorkflowStateManager.GetWorkflowVariable(context, options.TargetKey, cancellationToken);

                workflowVariable.SetValue(fact);

                await WorkflowStateManager.SetWorkflowVariable(context, workflowVariable, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            var test = ex.Message;
        }
    }

    private async Task<HttpResponseMessage> SendRequest(CatFactsNodeOptions options,
                                                        IWorkflowContext context,
                                                        CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get,
                                             BuildUri(options, context, cancellationToken));

        var client = _httpClientFactory.CreateClient($"{nameof(CatFactsNode)}HttpClient");

        return await client.SendAsync(request, cancellationToken);
    }

    private Uri BuildUri(CatFactsNodeOptions options, IWorkflowContext context, CancellationToken cancellationToken)
    {
        var queryParams = new Dictionary<string, string?>()
        {
            { MaxLengthParam, options.MaxLength.ToString()}
        };

        return new Uri(QueryHelpers.AddQueryString(BaseRequestUrl, queryParams));
    }

    public override WorkflowBuilder Configure(WorkflowBuilder builder,
                                              Dictionary<Guid, ParleyNode<ParleyLink>> nodes)
    {
        builder.AddEdge<ParleyLink>(this,
                                    nodes.Single(x => x.Key == NodeConfig.PrimaryTransitionNode).Value,
                                    link => link?.TransitionNode == NodeConfig.PrimaryTransitionNode);

        builder.AddEdge<ParleyLink>(this,
                                    nodes.Single(x => x.Key == NodeConfig.SecondaryTransitionNode).Value,
                                    link => link?.TransitionNode == NodeConfig.SecondaryTransitionNode);

        return builder;
    }
}