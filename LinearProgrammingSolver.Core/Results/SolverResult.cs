using System;
using System.Collections.Generic;
using System.Text;

namespace LinearProgrammingSolver.Core.Results
{
    public enum SolutionStatus
    {
        Optimal,
        Unbounded,
        Infeasible
    }
    public class SolverResult
    {
        public SolutionStatus Status { get; set; }
        public double OptimalValue { get; set; }
        public double[] VariableValues { get; set; }
        public double[] OptimalObjectiveRow { get; set; }
        public double[,] InverseBasis { get; set; }
        public double[,] FinalTableau { get; set; }
    }
}
