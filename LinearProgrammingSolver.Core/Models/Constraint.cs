using System.Collections.Generic;

namespace LinearProgrammingSolver.Core.Models
{
    public enum Relation
    {
        LessThanOrEqual,    // <=
        GreaterThanOrEqual, // >=
        Equal               // =
    }

    public class Constraint
    {
        public string Name { get; set; }
        public List<double> Coefficients { get; set; } = new List<double>();
        public Relation Relation { get; set; }
        public double RHS { get; set; } // RHS Value

        public Constraint(string name, List<double> coefficients, Relation relation, double rhs)
        {
            Name = name;
            Coefficients = coefficients;
            Relation = relation;
            RHS = rhs;
        }
    }
}