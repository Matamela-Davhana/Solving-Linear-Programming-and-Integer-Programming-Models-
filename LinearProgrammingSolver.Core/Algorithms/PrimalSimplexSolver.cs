using System;
using System.Collections.Generic;
using System.Text;

namespace LinearProgrammingSolver.Core
{
    //solves a linear programming question using Primal Simplex algorithm.
    public class PrimalSimplexSolver
    {
        private readonly Outwriter writer;

        public PrimalSimplexSolver(OutputWriter writer)
        {
            this.writer = writer;
        }

        public SolverResult Solve(LinearProgrammingSolver lp)
        {
            writer.WriteHeader("Primal Simplex Solver");

            //trigger the canonical from conversion
            lp.ConvertToCanonicalForm();

            int numVars = lp.NumDecisionVariables;
            int numConstraints = lp.NumConstraints;
            int totalCol = lp.variables.Count + 1; //columns for all variables including the RHS
            int totalRows = numConstraints + 1; //rows from z with all constraint rows

            double[,] tableau = new double[totalRows, totalCol];

            //populating obj
            for(int i = 0; i < numVars; i++)
            {
                tableau[0, j] = (lp.objective.Type == ObjectiveType.Maximize)
                    ? -lp.Objective.Coefficients[i] : lp.Objective.Coefficients[i];
            }

            //populating constraints
            for (int i = 0; i < numConstraints; i++) 
            {
                var constraints = lp.Constraints[i];

                for(int j = 0; j < constraints.Coefficients.Count; j++)
                {
                    tableau[i + 1, j] = constraints.Coefficients[j];
                }

                //rhs value
                tableau[i + 1, totalCol - 1] = constraints.RHS;
            }

            //adding slack, surplus, artificial variables into table
            int varIndex = numVars;
            for(int i = 0; i < numConstraints; i++)
            {
                var c =lp.Constraints[i];

                if (c.Relation == Relation.LessThanOrEqual)
                {
                    tableau[i + 1, varIndex++] = 1.0; //for slack variable
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

            //log canonical Form table
            writer.WriteTableau("Canonical Form/ Initial Tableau", tableau);

            //Pivoting
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
                    writer.WriteHeader("Result: Solution is UNBOUNDED");
                    return new SolverResult { Status = SolutionStatus.Unbounded };
                }

                // Pivot Elimination
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

            // Extract Inverse Basis (B^-1) for Member 2 Sensitivity Analysis
            double[][] inverseBasis = new double[numConstraints][];
            for (int i = 0; i < numConstraints; i++)
            {
                inverseBasis[i] = new double[numConstraints];
                for (int j = 0; j < numConstraints; j++)
                {
                    inverseBasis[i][j] = tableau[i + 1, lp.NumDecisionVariables + j];
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