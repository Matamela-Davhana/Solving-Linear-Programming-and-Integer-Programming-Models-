using System.Collections.Generic;
using LinearProgrammingSolver.Core.Models;
using LinearProgrammingSolver.Core.Results;

namespace LinearProgrammingSolver.Core.Algorithms
{
    public enum FathomReason
    {
        NotFathomed,
        Infeasible,
        IntegerFeasible,
        BoundWorseThanIncumbent
    }

    public class BranchAndBoundNode
    {
        public int NodeId { get; set; }
        public int? ParentId { get; set; }
        public LinearProgram SubProblem { get; set; }
        public SolverResult Result { get; set; }
        public double ObjectiveValue { get; set; }
        public double[] VariableValues { get; set; }
        public bool IsFathomed { get; set; }
        public FathomReason FathomedBy { get; set; }
        public string BranchingDescription { get; set; }

        public BranchAndBoundNode(int nodeId, int? parentId, LinearProgram subProblem, string description = "Root Node")
        {
            NodeId = nodeId;
            ParentId = parentId;
            SubProblem = subProblem;
            BranchingDescription = description;
            IsFathomed = false;
            FathomedBy = FathomReason.NotFathomed;
        }
    }
}