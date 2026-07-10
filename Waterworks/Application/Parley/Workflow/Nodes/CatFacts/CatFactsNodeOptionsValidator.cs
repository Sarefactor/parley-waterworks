using Parley.Configuration.Attributes;
using Parley.Dtos.Schema;
using Parley.Validation;
using Parley.Validation.Enums;
using Parley.Workflows.Nodes;

namespace Application.Parley.Workflow.Nodes.CatFacts;

[ParleyNodeValidator]
public class CatFactsNodeOptionsValidator : ParleyNodeOptionsValidator
{
    public override string NodeType => nameof(CatFactsNode);

    public override bool Validate(Guid workflowId,
                                  NodeConfigDto dto,
                                  IReadOnlyCollection<WorkflowVariableDto> workflowVariables,
                                  ParleyValidationContext context)
    {
        var isValid = true;

        if (!TrySerialiseOptions<CatFactsNodeOptions>(dto.NodeOptions, out var options)
            || options == null)
        {
            context.AddNodeError(workflowId,
                                 dto.NodeId,
                                 $"Encountered an error while serialising the options into {nameof(CatFactsNodeOptions)}.",
                                 WorkflowErrorType.Config,
                                 false);

            return false;
        }

        if (dto.SecondaryTransitionNode == Guid.Empty)
        {
            context.AddNodeError(workflowId,
                                 dto.NodeId,
                                 $"This node must have its secondary connector set.",
                                 WorkflowErrorType.Schema,
                                 false);

            return false;

        }

        if (options.MaxLength < 15)
        {
            context.AddNodeError(workflowId,
                                 dto.NodeId,
                                 $"Invalid value for {nameof(CatFactsNodeOptions)} property {nameof(options.MaxLength)}. Must be set to at least 15.",
                                 WorkflowErrorType.Config,
                                 false);

            isValid = false;
        }

        return isValid;
    }
}