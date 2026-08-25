using System;
using System.Collections.Generic;
using System.Linq;
using LinearProgrammingSolver.Core.Models;
using LinearProgrammingSolver.Core.Results;

namespace LinearProgrammingSolver.Core.Algorithms
{
    public class BranchAndBoundSimplexSolver
    {
        private readonly PrimalSimplexSolver _simplexSolver;
        public List<BranchAndBoundNode> AllNodes { get; private set; }
        public BranchAndBoundNode? BestCandidate { get; private set; }
        public double BestObjectiveValue { get; private set; }

        public BranchAndBoundSimplexSolver(PrimalSimplexSolver simplexSolver)
        {
            _simplexSolver = simplexSolver;
            AllNodes = new List<BranchAndBoundNode>();
        }

        public BranchAndBoundNode? Solve(LinearProgram initialModel)
        {
            AllNodes.Clear();
            bool isMax = initialModel.Objective.Type == ObjectiveType.Maximize;
            BestObjectiveValue = isMax ? double.NegativeInfinity : double.PositiveInfinity;
            BestCandidate = null;

            int nodeIdCounter = 0;
            var nodeStack = new Stack<BranchAndBoundNode>();

            // 1. Initialize Root Node (LP Relaxation)
            var rootNode = new BranchAndBoundNode(nodeIdCounter++, null, initialModel.Clone(), "Root Node");
            nodeStack.Push(rootNode);

            while (nodeStack.Count > 0)
            {
                // Backtracking (Depth-First Search)
                var currentNode = nodeStack.Pop();
                AllNodes.Add(currentNode);

                // Solve the LP relaxation of this subproblem
                currentNode.Result = _simplexSolver.Solve(currentNode.SubProblem);

                // Fathom Rule 1: Infeasible
                if (currentNode.Result == null || currentNode.Result.Status == SolutionStatus.Infeasible)
                {
                    currentNode.IsFathomed = true;
                    currentNode.FathomedBy = FathomReason.Infeasible;
                    continue;
                }

                currentNode.ObjectiveValue = currentNode.Result.OptimalValue;
                currentNode.VariableValues = currentNode.Result.VariableValues;

                // Fathom Rule 2: Bound is worse than current incumbent
                if (BestCandidate != null)
                {
                    if (isMax && currentNode.ObjectiveValue <= BestObjectiveValue + 1e-5)
                    {
                        currentNode.IsFathomed = true;
                        currentNode.FathomedBy = FathomReason.BoundWorseThanIncumbent;
                        continue;
                    }
                    if (!isMax && currentNode.ObjectiveValue >= BestObjectiveValue - 1e-5)
                    {
                        currentNode.IsFathomed = true;
                        currentNode.FathomedBy = FathomReason.BoundWorseThanIncumbent;
                        continue;
                    }
                }

                // Check for fractional values on integer/binary restricted variables
                int fractionalIndex = FindFractionalVariableIndex(currentNode.SubProblem, currentNode.VariableValues);

                if (fractionalIndex == -1)
                {
                    // Fathom Rule 3: Integrality achieved -> update Best Candidate
                    currentNode.IsFathomed = true;
                    currentNode.FathomedBy = FathomReason.IntegerFeasible;

                    if ((isMax && currentNode.ObjectiveValue > BestObjectiveValue) ||
                        (!isMax && currentNode.ObjectiveValue < BestObjectiveValue))
                    {
                        BestObjectiveValue = currentNode.ObjectiveValue;
                        BestCandidate = currentNode;
                    }
                    continue;
                }

                // Branching: Split on the fractional variable
                double fracValue = currentNode.VariableValues[fractionalIndex];
                double floorVal = Math.Floor(fracValue);
                double ceilVal = Math.Ceiling(fracValue);
                int numVars = currentNode.SubProblem.NumDecisionVariables;

                // Create Left Child constraint: x_j <= floor(value)
                var leftCoeffs = new List<double>(new double[numVars]);
                leftCoeffs[fractionalIndex] = 1.0;
                var leftConstraint = new Constraint($"Branch_x{fractionalIndex + 1}_LE", leftCoeffs, Relation.LessThanOrEqual, floorVal);

                var leftModel = currentNode.SubProblem.Clone();
                leftModel.Constraints.Add(leftConstraint);
                var leftNode = new BranchAndBoundNode(nodeIdCounter++, currentNode.NodeId, leftModel, $"x{fractionalIndex + 1} <= {floorVal}");

                // Create Right Child constraint: x_j >= ceil(value)
                var rightCoeffs = new List<double>(new double[numVars]);
                rightCoeffs[fractionalIndex] = 1.0;
                var rightConstraint = new Constraint($"Branch_x{fractionalIndex + 1}_GE", rightCoeffs, Relation.GreaterThanOrEqual, ceilVal);

                var rightModel = currentNode.SubProblem.Clone();
                rightModel.Constraints.Add(rightConstraint);
                var rightNode = new BranchAndBoundNode(nodeIdCounter++, currentNode.NodeId, rightModel, $"x{fractionalIndex + 1} >= {ceilVal}");

                // Push Right then Left onto stack (so Left is popped and explored first)
                nodeStack.Push(rightNode);
                nodeStack.Push(leftNode);
            }

            return BestCandidate;
        }

        private int FindFractionalVariableIndex(LinearProgram model, double[] variableValues)
        {
            if (variableValues == null || variableValues.Length == 0) return -1;

            int decisionVarCount = model.NumDecisionVariables;
            for (int i = 0; i < decisionVarCount && i < variableValues.Length; i++)
            {
                // Check if variable is restricted to Integer or Binary
                bool isIntegerRestricted = false;
                if (i < model.Variables.Count)
                {
                    var restriction = model.Variables[i].Restriction;
                    isIntegerRestricted = (restriction == SignRestriction.Integer || restriction == SignRestriction.Binary);
                }
                else
                {
                    // Default fallback: treat all decision variables as integer candidate if not specified
                    isIntegerRestricted = true;
                }

                if (isIntegerRestricted)
                {
                    double val = variableValues[i];
                    double rounded = Math.Round(val);
                    if (Math.Abs(val - rounded) > 1e-4)
                    {
                        return i; // Returns zero-based index of first fractional variable
                    }
                }
            }
            return -1;
        }
    }
}