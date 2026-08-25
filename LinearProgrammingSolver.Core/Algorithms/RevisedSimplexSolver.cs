using System;
using System.Collections.Generic;
using LinearProgrammingSolver.Core.IO; //using interface
using LinearProgrammingSolver.Core.Models;
using LinearProgrammingSolver.Core.Results;

namespace LinearProgrammingSolver.Core.Algorithms
{
    public class RevisedSimplexSolver
    {
        private readonly IOutputWriter _writer;

        public RevisedSimplexSolver(IOutputWriter writer)
        {
            _writer = writer;
        }

        public SolverResult Solve(LinearProgram lp)
        {
            _writer.WriteHeader("Revised Primal Simplex Solver");

            // Ensure canonical variables and structures are initialized
            lp.ConvertToCanonicalForm();

            int numVars = lp.NumDecisionVariables;
            int numConstraints = lp.NumConstraints;
            int totalVars = lp.Variables.Count;

            // Initial Basis Inverse (Identity Matrix I_m)
            double[,] BInverse = new double[numConstraints, numConstraints];
            for (int i = 0; i < numConstraints; i++) BInverse[i, i] = 1.0;

            // Basic variable indices
            int[] basicVars = new int[numConstraints];
            int varTracker = numVars;
            for (int i = 0; i < numConstraints; i++)
            {
                basicVars[i] = varTracker;
                varTracker += (lp.Constraints[i].Relation == Relation.GreaterThanOrEqual) ? 2 : 1;
            }

            // Vector c (Objective coefficients mapped for decision variables)
            double[] c = new double[totalVars];
            for (int j = 0; j < numVars; j++)
            {
                c[j] = (lp.Objective.Type == ObjectiveType.Maximize)
                    ? lp.Objective.Coefficients[j]
                    : -lp.Objective.Coefficients[j];
            }

            // Matrix A construction (Decision + Slacks/Surplus/Artificials)
            double[,] A = new double[numConstraints, totalVars];
            int slackColIndex = numVars;

            for (int i = 0; i < numConstraints; i++)
            {
                var constraint = lp.Constraints[i];

                // Coefficients for decision variables
                for (int j = 0; j < constraint.Coefficients.Count; j++)
                {
                    A[i, j] = constraint.Coefficients[j];
                }

                // Add structural slack/surplus/artificial variables
                if (constraint.Relation == Relation.LessThanOrEqual)
                {
                    A[i, slackColIndex++] = 1.0;
                }
                else if (constraint.Relation == Relation.GreaterThanOrEqual)
                {
                    A[i, slackColIndex++] = -1.0; // Surplus
                    A[i, slackColIndex++] = 1.0;  // Artificial
                }
                else if (constraint.Relation == Relation.Equal)
                {
                    A[i, slackColIndex++] = 1.0;  // Artificial
                }
            }

            double[] b = new double[numConstraints];
            for (int i = 0; i < numConstraints; i++) b[i] = lp.Constraints[i].RHS;

            int iteration = 0;

            while (true)
            {
                iteration++;

                // Step 1: Compute Price-Out Vector (pi = c_B * B^-1)
                double[] c_B = new double[numConstraints];
                for (int i = 0; i < numConstraints; i++) c_B[i] = c[basicVars[i]];

                double[] pi = new double[numConstraints];
                for (int i = 0; i < numConstraints; i++)
                {
                    for (int j = 0; j < numConstraints; j++)
                        pi[i] += c_B[j] * BInverse[j, i];
                }

                _writer.WriteArray($"Iteration {iteration} - Price-Out Vector (pi)", pi);

                // Step 2: Compute Reduced Costs (z_j - c_j)
                int enteringCol = -1;
                double maxNetEvaluation = 0.00001;

                for (int j = 0; j < totalVars; j++)
                {
                    double pi_A_j = 0;
                    for (int i = 0; i < numConstraints; i++) pi_A_j += pi[i] * A[i, j];

                    double netEval = c[j] - pi_A_j;
                    if (netEval > maxNetEvaluation)
                    {
                        maxNetEvaluation = netEval;
                        enteringCol = j;
                    }
                }

                if (enteringCol == -1)
                {
                    // Optimal Solution Found
                    double objValue = 0;
                    double[] b_bar = MultiplyMatrixVector(BInverse, b);
                    for (int i = 0; i < numConstraints; i++) objValue += c_B[i] * b_bar[i];

                    _writer.WriteHeader("Optimal Solution Reached");
                    return new SolverResult
                    {
                        Status = SolutionStatus.Optimal,
                        OptimalValue = objValue,
                        InverseBasis = BInverse,
                        OptimalObjectiveRow = pi
                    };
                }

                // Step 3: Compute Column for Entering Variable (d_k = B^-1 * A_k)
                double[] A_k = new double[numConstraints];
                for (int i = 0; i < numConstraints; i++) A_k[i] = A[i, enteringCol];
                double[] d_k = MultiplyMatrixVector(BInverse, A_k);

                // Step 4: Minimum Ratio Test
                double[] x_B = MultiplyMatrixVector(BInverse, b);
                int leavingRow = -1;
                double minRatio = double.MaxValue;

                for (int i = 0; i < numConstraints; i++)
                {
                    if (d_k[i] > 0.00001)
                    {
                        double ratio = x_B[i] / d_k[i];
                        if (ratio < minRatio)
                        {
                            minRatio = ratio;
                            leavingRow = i;
                        }
                    }
                }

                if (leavingRow == -1)
                {
                    _writer.WriteHeader("Result: UNBOUNDED Solution");
                    return new SolverResult { Status = SolutionStatus.Unbounded };
                }

                // Step 5: ETA Matrix (E_k) Generation & B^-1 Update
                double[,] E_k = new double[numConstraints, numConstraints];
                for (int i = 0; i < numConstraints; i++) E_k[i, i] = 1.0;

                for (int i = 0; i < numConstraints; i++)
                {
                    if (i == leavingRow)
                        E_k[i, leavingRow] = 1.0 / d_k[leavingRow];
                    else
                        E_k[i, leavingRow] = -d_k[i] / d_k[leavingRow];
                }

                BInverse = MultiplyMatrices(E_k, BInverse);
                basicVars[leavingRow] = enteringCol;
            }
        }

        private double[] MultiplyMatrixVector(double[,] matrix, double[] vector)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double[] result = new double[rows];
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    result[i] += matrix[i, j] * vector[j];
            return result;
        }

        private double[,] MultiplyMatrices(double[,] A, double[,] B)
        {
            int rowsA = A.GetLength(0);
            int colsA = A.GetLength(1);
            int colsB = B.GetLength(1);
            double[,] result = new double[rowsA, colsB];

            for (int i = 0; i < rowsA; i++)
                for (int j = 0; j < colsB; j++)
                    for (int k = 0; k < colsA; k++)
                        result[i, j] += A[i, k] * B[k, j];

            return result;
        }

    }
}