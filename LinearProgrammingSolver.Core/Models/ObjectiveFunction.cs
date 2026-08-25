using System.Collections.Generic;

namespace LinearProgrammingSolver.Core.Models
{
    public enum ObjectiveType
    {
        Maximize,
        Minimize
    }

    public class ObjectiveFunction
    {
        public ObjectiveType Type { get; set; }
        public List<double> Coefficients { get; set; } = new List<double>();

        public ObjectiveFunction(ObjectiveType type, List<double> coefficients)
        {
            Type = type;
            Coefficients = coefficients;
        }
    }
}