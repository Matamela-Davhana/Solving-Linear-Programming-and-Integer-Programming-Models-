using System.Collections.Generic;

namespace LinearProgrammingSolver.Core
{
    //holds the optimized objective function and original coefficients c
    public enum OptimizationType
    {
        Maximize,
        Minimize
    }

    public class ObjectiveFunction
    {
        public OptimizationType optimizationType {  get; set; }
        public List<double> Coefficients { get; set; } = new List<double> ();

        public ObjectiveFunction(OptimizationType optimizationtype, List<double> coefficients)
        {
            optimizationType = optimizationtype;
            Coefficients = coefficients;
        }
    }
}