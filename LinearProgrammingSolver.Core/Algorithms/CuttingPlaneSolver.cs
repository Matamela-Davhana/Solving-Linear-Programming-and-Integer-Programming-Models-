using System;
using System.Collections.Generic;
using System.Linq;
using LinearProgrammingSolver.Core.Models;
using LinearProgrammingSolver.Core.Results;

namespace LinearProgrammingSolver.Core.Algorithms
{
    public class CuttingPlaneSolver
    {
        private const double EPSILON = 1e-6;
        private readonly PrimalSimplexSolver _simplexSolver;
        public List<CuttingPlaneIteration> Iterations { get; private set; }
        public SolverResult? FinalResult { get; private set; }

        public class CuttingPlaneIteration
        {
            public int IterationNumber { get; set; }
            public double ObjectiveValue { get; set; }
            public double[] VariableValues { get; set; }
            public bool IsInteger { get; set; }
            public bool CutAdded { get; set; }
            public string CutDescription { get; set; }

            public CuttingPlaneIteration()
            {
                VariableValues = Array.Empty<double>();
                CutDescription = "";
            }
        }

        public CuttingPlaneSolver(PrimalSimplexSolver simplexSolver)
        {
            _simplexSolver = simplexSolver;
            Iterations = new List<CuttingPlaneIteration>();
        }

        public SolverResult Solve(LinearProgram initialModel)
        {
            if (initialModel == null)
            {
                throw new ArgumentNullException(nameof(initialModel));
            }

            ValidateModel(initialModel);
            Iterations.Clear();
            FinalResult = null;

            LinearProgram workingModel = initialModel.Clone();
            int iteration = 0;

            while (iteration < 100)
            {
                iteration++;

                Console.WriteLine();
                Console.WriteLine("==================================================");
                Console.WriteLine($"CUTTING PLANE ITERATION {iteration}");
                Console.WriteLine("==================================================");

                LinearProgram relaxation = CreateRelaxation(workingModel);
                SolverResult result = _simplexSolver.Solve(relaxation);

                if (result == null || result.Status == SolutionStatus.Infeasible)
                {
                    FinalResult = new SolverResult
                        {
                            Status = SolutionStatus.Infeasible,
                            OptimalValue = double.NaN,
                            VariableValues = Array.Empty<double>()
                        };

                    return FinalResult;
                }

                if (result.Status == SolutionStatus.Unbounded)
                {
                    FinalResult = result;
                    return FinalResult;
                }

                bool isInteger = IsIntegerSolution(workingModel, result.VariableValues);
                var iterationData = new CuttingPlaneIteration
                    {
                        IterationNumber = iteration,
                        ObjectiveValue = result.OptimalValue,
                        VariableValues = result.VariableValues.Take(workingModel.NumDecisionVariables).ToArray(),
                        IsInteger = isInteger,
                        CutAdded = false,
                        CutDescription = ""
                    };

                if (isInteger)
                {
                    iterationData.CutAdded = false;
                    iterationData.CutDescription = "Integer solution found.";
                    Iterations.Add(iterationData);
                    FinalResult = result;

                    Console.WriteLine("Integer solution found.");
                    return FinalResult;
                }

                int fractionalIndex = FindFractionalVariable(workingModel, result.VariableValues);

                if (fractionalIndex == -1)
                {
                    iterationData.CutDescription = "No suitable fractional variable found.";
                    Iterations.Add(iterationData);
                    FinalResult = result;
                    return FinalResult;
                }

                double fractionalValue = result.VariableValues[fractionalIndex];
                double floorValue = Math.Floor(fractionalValue);
                string variableName = workingModel.Variables[fractionalIndex].Name;
                var coefficients = new List<double>(new double[workingModel.NumDecisionVariables]);
                coefficients[fractionalIndex] = 1.0;

                Constraint cut = new Constraint($"Cut_{iteration}", coefficients, Relation.LessThanOrEqual, floorValue);
                workingModel.Constraints.Add(cut);
                iterationData.CutAdded = true;
                iterationData.CutDescription = $"{variableName} <= " + $"{floorValue:F4}";
                Iterations.Add(iterationData);

                Console.WriteLine($"Fractional variable: " + $"{variableName} = " + $"{fractionalValue:F4}");
                Console.WriteLine($"Cut added: " + $"{variableName} <= " + $"{floorValue:F4}");
            }

            FinalResult = new SolverResult
            {
                    Status = SolutionStatus.Infeasible,
                    OptimalValue = double.NaN,
                    VariableValues = Array.Empty<double>()
            };
            return FinalResult;
        }

        private LinearProgram CreateRelaxation(LinearProgram model)
        {
            LinearProgram relaxation = model.Clone();

            foreach (Variable variable in relaxation.Variables)
            {
                if (variable.Type == VariableType.Decision)
                {
                    var originalRestriction = variable.Restriction;
                    if (originalRestriction == SignRestriction.Integer || originalRestriction == SignRestriction.Binary)
                      {
                         variable.Restriction = SignRestriction.Positive;
                         variable.LowerBound = 0.0;
                        
                           if (originalRestriction == SignRestriction.Binary)
                               variable.UpperBound = 1.0;
                      }
                }
            }
            return relaxation;
        }

        private bool IsIntegerSolution(LinearProgram model, double[] values)
        {
            for (int i = 0; i < model.NumDecisionVariables; i++)
            {
                Variable variable = model.Variables[i];

                if (variable.Restriction != SignRestriction.Integer && variable.Restriction != SignRestriction.Binary)
                {
                    continue;
                }

                double value = values[i];

                if (variable.Restriction == SignRestriction.Binary)
                {
                    bool zero = Math.Abs(value) <= EPSILON;
                    bool one = Math.Abs(value - 1.0) <= EPSILON;

                    if (!zero && !one)
                    {
                        return false;
                    }
                }

                else
                {
                    double rounded = Math.Round(value);

                    if (Math.Abs(value - rounded) > EPSILON)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private int FindFractionalVariable(LinearProgram model, double[] values)
        {
            for (int i = 0; i < model.NumDecisionVariables; i++)
            {
                Variable variable = model.Variables[i];

                if (variable.Restriction != SignRestriction.Integer && variable.Restriction != SignRestriction.Binary)
                {
                    continue;
                }

                double value = values[i];

                if (variable.Restriction == SignRestriction.Binary)
                {
                    bool zero = Math.Abs(value) <= EPSILON;
                    bool one = Math.Abs(value - 1.0) <= EPSILON;

                    if (!zero && !one)
                    {
                        return i;
                    }
                }

                else
                {
                    double rounded = Math.Round(value);

                    if (Math.Abs(value - rounded) > EPSILON)
                    {
                        return i;
                    }
                }

                if (!iterationData.CutAdded)
                     break;
            }
            return -1;
        }

        private void ValidateModel(LinearProgram model)
        {
            if (model.Objective == null)
            {
                throw new ArgumentException("An objective function is required.");
            }

            if (model.NumDecisionVariables == 0)
            {
                throw new ArgumentException("At least one decision variable is required.");
            }

            bool hasIntegerVariable = model.Variables.Any(v => v.Restriction == SignRestriction.Integer ||
                                      v.Restriction == SignRestriction.Binary);

            if (!hasIntegerVariable)
            {
                throw new ArgumentException("Cutting Plane requires at least " + "one Integer or Binary variable.");
            }
        }
        
        public void PrintResult(SolverResult result)
        {
            Console.WriteLine();
            Console.WriteLine("==============================================================");
            Console.WriteLine("                 CUTTING PLANE RESULT");
            Console.WriteLine("==============================================================");
            Console.WriteLine($"Status      : {result.Status}");

            if (result.Status == SolutionStatus.Optimal)
            {
                Console.WriteLine($"Optimal Z   : " + $"{result.OptimalValue:F3}");
                Console.WriteLine();
                Console.WriteLine("Variable Values");
                Console.WriteLine("--------------------------------------------------------------");

                for (int i = 0; i < result.VariableValues.Length; i++)
                {
                    Console.WriteLine($"x{i + 1} = " + $"{result.VariableValues[i]:F3}");
                }
            }

            Console.WriteLine();
            Console.WriteLine( "CUTTING PLANE ITERATIONS");
            Console.WriteLine("================================================================================================");
            Console.WriteLine(
                $"{"Iteration",-12}" +
                $"{"LP Z",-15}" +
                $"{"Integer",-12}" +
                $"{"Cut Added",-12}" +
                $"{"Cut",-35}");
            Console.WriteLine("================================================================================================");

            foreach (CuttingPlaneIteration data in Iterations)
            {
                Console.WriteLine(
                    $"{data.IterationNumber,-12}" +
                    $"{data.ObjectiveValue,-15:F3}" +
                    $"{data.IsInteger,-12}" +
                    $"{data.CutAdded,-12}" +
                    $"{data.CutDescription,-35}");
            }
            Console.WriteLine("================================================================================================");
        }
    }
}
