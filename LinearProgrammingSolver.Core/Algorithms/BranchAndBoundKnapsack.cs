using System;
using System.Collections.Generic;
using System.Linq;
using LinearProgrammingSolver.Core.Models;
using LinearProgrammingSolver.Core.Results;

namespace LinearProgrammingSolver.Core.Algorithms
{
    public class BnBKnapsack
    {
        private const double EPSILON = 1e-6;
        private readonly PrimalSimplexSolver _simplexSolver;
        private readonly List<SubProblem> _subProblems = new List<SubProblem>();
        private double _bestObjectiveValue;
        private double[] _bestVariableValues = Array.Empty<double>();
        private SubProblem? _bestSubProblem;
        private bool _isMaximize;

        public class SubProblem
        {
            public string Number { get; set; }
            public string ParentNumber { get; set; }
            public LinearProgram Model { get; set; }
            public double ObjectiveValue { get; set; }
            public double[] VariableValues { get; set; }
            public bool IsBranched { get; set; }
            public string BranchedBy { get; set; }
            public string BranchDescription { get; set; }

            public SubProblem(string number, string parentNumber, LinearProgram model, string branchDescription)
            {
                Number = number;
                ParentNumber = parentNumber;
                Model = model;
                ObjectiveValue = 0.0;
                VariableValues = Array.Empty<double>();
                IsBranched = false;
                BranchedBy = "";
                BranchDescription = branchDescription;
            }
        }

        public class Result
        {
            public SolutionStatus Status { get; set; }
            public double OptimalValue { get; set; }
            public double[] VariableValues { get; set; }
            public SubProblem? BestSubProblem { get; set; }

            public Result()
            {
                VariableValues = Array.Empty<double>();
            }
        }

        public BnBKnapsack(PrimalSimplexSolver simplexSolver)
        {
            _simplexSolver = simplexSolver;
        }

        public Result Solve(LinearProgram initialModel)
        {
            ValidateKnapsack(initialModel);
            _subProblems.Clear();
            _bestSubProblem = null;
            _bestVariableValues = Array.Empty<double>();

            ObjectiveFunction objective = initialModel.Objective;
            _isMaximize = objective.Type == ObjectiveType.Maximize;
            Constraint knapsackConstraint = initialModel.Constraints[0];
            
            double[] profits = objective.Coefficients.ToArray();
            double[] weights = knapsackConstraint.Coefficients.ToArray();
            double capacity = knapsackConstraint.RHS;

            // These are obtained directly from the LP.
            // They are not recreated by BnBKnapsack.

            Console.WriteLine();
            Console.WriteLine("Knapsack Problem");
            Console.WriteLine("----------------------------");
            Console.WriteLine();
            Console.WriteLine($"Objective Type : {objective.Type}");
            Console.WriteLine($"Capacity = {capacity}");
            Console.WriteLine("Profits = " + string.Join(", ", profits));
            Console.WriteLine("Weights = " + string.Join(", ", weights));
            Console.WriteLine();

            _bestObjectiveValue = _isMaximize ? double.NegativeInfinity : double.PositiveInfinity;

            var root =
                new SubProblem("0", "", initialModel.Clone(), "Root Subproblem");

            var stack = new Stack<SubProblem>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                SubProblem current = stack.Pop();

                LinearProgram relaxation = CreateRelaxation(current.Model);
                SolverResult? lpResult = _simplexSolver.Solve(relaxation);

                if (lpResult == null || lpResult.Status == SolutionStatus.Infeasible)
                {
                    current.IsBranched = true;
                    current.BranchedBy = "Infeasible";
                    _subProblems.Add(current);
                    continue;
                }

                if (lpResult.Status == SolutionStatus.Unbounded)
                {
                    current.IsBranched = true;
                    current.BranchedBy = "Unbounded";
                    _subProblems.Add(current);
                    continue;
                }

                current.ObjectiveValue = lpResult.OptimalValue;
                current.VariableValues = lpResult.VariableValues;

                if (_bestSubProblem != null)
                {
                    if (_isMaximize && current.ObjectiveValue <= _bestObjectiveValue + EPSILON)
                    {
                        current.IsBranched = true;
                        current.BranchedBy = "Bound";
                        _subProblems.Add(current);
                        continue;
                    }

                    if (!_isMaximize && current.ObjectiveValue >= _bestObjectiveValue - EPSILON)
                    {
                        current.IsBranched = true;
                        current.BranchedBy = "Bound";
                        _subProblems.Add(current);
                        continue;
                    }
                }

                int fractionalIndex = FindFractionalBinaryVariable(current.Model, current.VariableValues);

                if (fractionalIndex == -1)
                {
                    current.IsBranched = true;
                    current.BranchedBy = "Binary Feasible";
                    UpdateBestSolution(current);
                    _subProblems.Add(current);
                    continue;
                }

                string variableName = current.Model.Variables[fractionalIndex].Name;

                string childOneNumber = GetChildNumber(current.Number, 1);
                LinearProgram childOneModel = current.Model.Clone();

                childOneModel.Constraints.Add(CreateBinaryBranchConstraint(childOneModel.NumDecisionVariables,
                        fractionalIndex, 0, $"Branch_{variableName}_EQ_0"));

                var childOne = new SubProblem(childOneNumber, current.Number, childOneModel, $"{variableName} = 0");

                string childTwoNumber = GetChildNumber(current.Number, 2);
                LinearProgram childTwoModel = current.Model.Clone();

                childTwoModel.Constraints.Add(CreateBinaryBranchConstraint(childTwoModel.NumDecisionVariables,
                        fractionalIndex, 1,"Branch_{variableName}_EQ_1"));

                var childTwo = new SubProblem(childTwoNumber, current.Number, childTwoModel, $"{variableName} = 1");

                stack.Push(childTwo);
                stack.Push(childOne);

                // The current subproblem was branched.
                current.IsBranched = true;
                current.BranchedBy = "Branched";
                _subProblems.Add(current);
            }

            if (_bestSubProblem == null)
            {
                return new Result
                {
                    Status = SolutionStatus.Infeasible,
                    OptimalValue = double.NaN,
                    VariableValues = Array.Empty<double>(),
                    BestSubProblem = null
                };
            }

            return new Result
            {
                Status = SolutionStatus.Optimal,
                OptimalValue = _bestObjectiveValue,
                VariableValues = _bestVariableValues,
                BestSubProblem = _bestSubProblem
            };
        }

        private LinearProgram CreateRelaxation(LinearProgram model)
        {
            LinearProgram relaxation = model.Clone();

            foreach (Variable variable in relaxation.Variables)
            {
                if (variable.Type == VariableType.Decision && variable.Restriction == SignRestriction.Binary)
                {
                    variable.Restriction = SignRestriction.Positive;
                    variable.LowerBound = 0.0;
                    variable.UpperBound = 1.0;
                }
            }
            return relaxation;
        }

        private int FindFractionalBinaryVariable(LinearProgram model, double[] values)
        {
            for (int i = 0; i < model.NumDecisionVariables; i++)
            {
                Variable variable = model.Variables[i];

                if (variable.Restriction != SignRestriction.Binary)
                {
                    continue;
                }

                double value = values[i];
                bool isZero = Math.Abs(value) <= EPSILON;
                bool isOne = Math.Abs(value - 1.0) <= EPSILON;

                if (!isZero && !isOne)
                {
                    return i;
                }
            }
            return -1;
        }

        private Constraint CreateBinaryBranchConstraint(int variableCount, int variableIndex, int value, string name)
        {
            var coefficients = new List<double>(new double[variableCount]);
            coefficients[variableIndex] = 1.0;

            return new Constraint( name, coefficients, Relation.Equal, value);
        }

        private string GetChildNumber(string parentNumber, int childNumber)
        {
            if (parentNumber == "0")
            {
                return childNumber.ToString();
            }

            return parentNumber + "." + childNumber;
        }

        private void UpdateBestSolution(SubProblem subProblem)
        {
            double value = subProblem.ObjectiveValue;
            bool better;

            if (_bestSubProblem == null)
            {
                better = true;
            }
            else if (_isMaximize)
            {
                better = value > _bestObjectiveValue + EPSILON;
            }
            else
            {
                better = value < _bestObjectiveValue - EPSILON;
            }
            if (!better)
            {
                return;
            }

            _bestObjectiveValue = value;
            _bestVariableValues = subProblem.VariableValues.Take(subProblem.Model.NumDecisionVariables).ToArray();
            _bestSubProblem = subProblem;
        }

        private void ValidateKnapsack(LinearProgram model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (model.Objective == null)
            {
                throw new ArgumentException("Objective function is required.");
            }

            if (model.Objective.Type != ObjectiveType.Maximize)
            {
                throw new ArgumentException("Binary knapsack must be a maximization problem.");
            }

            if (model.Constraints.Count != 1)
            {
                throw new ArgumentException("Binary knapsack must have exactly one constraint.");
            }

            Constraint constraint = model.Constraints[0];

            if (constraint.Relation != Relation.LessThanOrEqual)
            {
                throw new ArgumentException("Knapsack constraint must use <=.");
            }

            if (constraint.RHS < 0)
            {
                throw new ArgumentException("Knapsack capacity cannot be negative.");
            }

            if (constraint.Coefficients.Count != model.NumDecisionVariables)
            {
                throw new ArgumentException("Number of constraint coefficients " + "must match number of decision variables.");
            }

            for (int i = 0; i < model.NumDecisionVariables; i++)
            {
                Variable variable = model.Variables[i];

                if (variable.Restriction != SignRestriction.Binary)
                {
                    throw new ArgumentException($"{variable.Name} must be Binary.");
                }

                if (constraint.Coefficients[i] < 0)
                {
                    throw new ArgumentException($"Weight for {variable.Name} " + "cannot be negative.");
                }
            }
        }

        public void PrintResult(
            Result result)
        {
            Console.WriteLine();
            Console.WriteLine("==============================================================");
            Console.WriteLine("                   FINAL SOLUTION");
            Console.WriteLine("==============================================================");
            Console.WriteLine($"Status             : {result.Status}");

            if (result.Status == SolutionStatus.Optimal)
            {
                Console.WriteLine($"Optimal Z          : " + $"{result.OptimalValue:F2}");

                if (result.BestSubProblem != null)
                {
                    Console.WriteLine($"Optimal Subproblem : " + $"{result.BestSubProblem.Number}");
                }

                Console.WriteLine();
                Console.WriteLine("Selected Items");
                Console.WriteLine("--------------------------------------------------------------");

                for (int i = 0; i < result.VariableValues.Length; i++)
                {
                    Console.WriteLine($"x{i + 1} = " + $"{result.VariableValues[i]:F0}");
                }
            }

            Console.WriteLine();
            Console.WriteLine("BRANCH-AND-BOUND SUBPROBLEMS");
            Console.WriteLine("================================================================================================");
            Console.WriteLine(
                $"{"Subproblem",-14}" +
                $"{"Parent",-14}" +
                $"{"LP Bound",-14}" +
                $"{"Branched",-14}" +
                $"{"Reason",-25}" +
                $"{"Branch",-25}");
            Console.WriteLine("================================================================================================");

            foreach (SubProblem subProblem in _subProblems)
            {
                Console.WriteLine(
                    $"{subProblem.Number,-14}" +
                    $"{(string.IsNullOrEmpty(subProblem.ParentNumber) ? "-" : subProblem.ParentNumber),-14}" +
                    $"{subProblem.ObjectiveValue,-14:F2}" +
                    $"{subProblem.IsBranched,-14}" +
                    $"{subProblem.BranchedBy,-25}" +
                    $"{subProblem.BranchDescription,-25}");
            }

            Console.WriteLine();
            Console.WriteLine("Branched means the subproblem does not need");
            Console.WriteLine("to be explored further; it does NOT necessarily");
            Console.WriteLine("mean that the subproblem itself is optimal.");
        }
    }
}
