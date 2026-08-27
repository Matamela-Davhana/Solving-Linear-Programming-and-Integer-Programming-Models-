using System;
using System.Collections.Generic;
using LinearProgrammingSolver.Core.IO; //using the interface
using LinearProgrammingSolver.Core.Models;
using LinearProgrammingSolver.Core.Results;

namespace LinearProgrammingSolver.Core.Algorithms
{
    // Solves a linear programming problem using Primal Simplex algorithm.
    public class PrimalSimplexSolver
    {
        private readonly IOutputWriter _writer;

        public PrimalSimplexSolver(IOutputWriter writer)
        {
            _writer = writer;
        }

        public SolverResult Solve(LinearProgram lp)
        {
            _writer.WriteHeader("Primal Simplex Solver");

            // Trigger canonical form conversion
            lp.ConvertToCanonicalForm();

            int numVars = lp.NumDecisionVariables;
            int numConstraints = lp.NumConstraints;
            int totalCols = lp.Variables.Count + 1; // Columns for all variables including RHS
            int totalRows = numConstraints + 1;    // Objective row (Z) + constraint rows

            double[,] tableau = new double[totalRows, totalCols];

            // Populate Objective Row (Row 0)
            for (int j = 0; j < numVars; j++)
            {
                tableau[0, j] = (lp.Objective.Type == ObjectiveType.Maximize)
                    ? -lp.Objective.Coefficients[j]
                    : lp.Objective.Coefficients[j];
            }

            // Populate Constraints Matrix
            for (int i = 0; i < numConstraints; i++)
            {
                var constraint = lp.Constraints[i];

                for (int j = 0; j < constraint.Coefficients.Count; j++)
                {
                    tableau[i + 1, j] = constraint.Coefficients[j];
                }

                // RHS value
                tableau[i + 1, totalCols - 1] = constraint.RHS;
            }

            // Add Slack, Surplus, and Artificial variables into Tableau
            int varIndex = numVars;
            for (int i = 0; i < numConstraints; i++)
            {
                var c = lp.Constraints[i];

                if (c.Relation == Relation.LessThanOrEqual)
                {
                    tableau[i + 1, varIndex++] = 1.0; // Slack variable
                }
                else if (c.Relation == Relation.GreaterThanOrEqual)
                {
                    tableau[i + 1, varIndex++] = -1.0; // Surplus
                    tableau[i + 1, varIndex++] = 1.0;  // Artificial
                }
                else if (c.Relation == Relation.Equal)
                {
                    tableau[i + 1, varIndex++] = 1.0;  // Artificial
                }
            }

            // Log Initial Canonical Form Tableau
            _writer.WriteTableau("Canonical Form / Initial Tableau", tableau);

            // Pivoting Execution Loop
            int iteration = 0;
            while (true)
            {
                iteration++;

                // Entering Variable (Pivot Column)
                int pivotCol = -1;
                double minVal = -0.00001;

                for (int j = 0; j < totalCols - 1; j++)
                {
                    if (tableau[0, j] < minVal)
                    {
                        minVal = tableau[0, j];
                        pivotCol = j;
                    }
                }

                if (pivotCol == -1)
                {
                    _writer.WriteTableau($"Final Optimal Tableau (Iteration {iteration - 1})", tableau);
                    return ExtractResults(lp, tableau, SolutionStatus.Optimal);
                }

                // Leaving Variable (Pivot Row - Minimum Ratio Test)
                int pivotRow = -1;
                double minRatio = double.MaxValue;

                for (int i = 1; i < totalRows; i++)
                {
                    double elem = tableau[i, pivotCol];
                    if (elem > 0.00001)
                    {
                        double ratio = tableau[i, totalCols - 1] / elem;
                        if (ratio < minRatio)
                        {
                            minRatio = ratio;
                            pivotRow = i;
                        }
                    }
                }

                if (pivotRow == -1)
                {
                    _writer.WriteHeader("Result: Solution is UNBOUNDED");
                    return new SolverResult { Status = SolutionStatus.Unbounded };
                }

                // Pivot Elimination (Gauss-Jordan)
                double pivotVal = tableau[pivotRow, pivotCol];
                for (int j = 0; j < totalCols; j++)
                    tableau[pivotRow, j] /= pivotVal;

                for (int i = 0; i < totalRows; i++)
                {
                    if (i != pivotRow)
                    {
                        double factor = tableau[i, pivotCol];
                        for (int j = 0; j < totalCols; j++)
                            tableau[i, j] -= factor * tableau[pivotRow, j];
                    }
                }

                _writer.WriteTableau($"Iteration {iteration}", tableau);
            }
        }

        private SolverResult ExtractResults(LinearProgram lp, double[,] tableau, SolutionStatus status)
        {
            int rows = tableau.GetLength(0);
            int cols = tableau.GetLength(1);
            int numConstraints = lp.NumConstraints;

            double[] optRow = new double[cols - 1];
            for (int j = 0; j < cols - 1; j++) optRow[j] = tableau[0, j];

            double optVal = tableau[0, cols - 1];
            double[] varVals = new double[lp.NumDecisionVariables];

            // Extract values of basic decision variables
            for (int j = 0; j < lp.NumDecisionVariables; j++)
            {
                int basicRow = -1;
                int countOnes = 0;
                bool isClean = true;

                for (int i = 1; i < rows; i++)
                {
                    if (Math.Abs(tableau[i, j] - 1.0) < 0.0001)
                    {
                        countOnes++;
                        basicRow = i;
                    }
                    else if (Math.Abs(tableau[i, j]) > 0.0001)
                    {
                        isClean = false;
                    }
                }

                if (countOnes == 1 && isClean && basicRow != -1)
                {
                    varVals[j] = tableau[basicRow, cols - 1];
                }
            }

            // Extract Inverse Basis Matrix (B^-1) for Sensitivity Analysis
            double[,] inverseBasis = new double[numConstraints, numConstraints];
            for (int i = 0; i < numConstraints; i++)
            {
                for(int j = 0; j < numConstraints; j++)
                {
                    inverseBasis[i, j] = tableau[i + 1, lp.NumDecisionVariables + j];
                }
                
            }

            return new SolverResult
            {
                Status = status,
                OptimalValue = optVal,
                VariableValues = varVals,
                OptimalObjectiveRow = optRow,
                InverseBasis = inverseBasis,
                FinalTableau = tableau
            };

        }
    }
}