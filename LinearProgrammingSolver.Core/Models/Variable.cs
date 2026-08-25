namespace LinearProgrammingSolver.Core.Models
{
    public enum VariableType
    {
        Decision,   // Standard decision variable (xi)
        Slack,      // For <= constraint (si)
        Surplus,    // For >= constraint (ei)
        Artificial  // For >= or = constraint (ai)
    }

    public enum SignRestriction
    {
        Positive,    // x >= 0
        Negative,    // x <= 0
        Unrestricted // urs
    }

    public class Variable
    {
        public string Name { get; set; }
        public VariableType Type { get; set; }
        public SignRestriction Restriction { get; set; }
        public double LowerBound { get; set; } = 0.0;
        public double UpperBound { get; set; } = double.PositiveInfinity;

        public Variable(string name, VariableType type = VariableType.Decision, SignRestriction restriction = SignRestriction.Positive)
        {
            Name = name;
            Type = type;
            Restriction = restriction;
        }
    }
}