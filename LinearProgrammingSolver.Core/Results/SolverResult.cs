using System;
using System.Collections.Generic;
using System.Text;

namespace LinearProgrammingSolver.Core.Results
{
    //final solution outcome of the LP algorithm
    public enum SolutionStatus
    {
        Optimal,
        Unbounded,
        Infeasible
    }
    public class SolverResult
    {
        public SolutionStatus Status { get; set; } // Tracks whether the solution reached an optimal, unbounded, or infeasible state
        public double OptimalValue { get; set; } // Stores the final value of the objective function Z
        public double[] VariableValues { get; set; } = Array.Empty<double>(); // Stores values for decision variables; defaults to an empty array to prevent null reference errors
        public double[] OptimalObjectiveRow { get; set; } = Array.Empty<double>(); // Stores final objective row; defaults to empty array to avoid null crashes
        public double[,] InverseBasis { get; set; } = new double[0, 0]; // Stores the B^-1 matrix needed for Sensitivity Analysis; defaults to an empty 2D matrix
        public double[,]? FinalTableau { get; set; } // Stores full tableau if using Primal Simplex; marked '?' because Revised Simplex does not produce one
    }
}
