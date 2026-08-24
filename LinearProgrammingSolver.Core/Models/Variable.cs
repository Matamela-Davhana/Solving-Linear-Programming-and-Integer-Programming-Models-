namespace LinearProgrammingSolver.Core
{
    public enum VariableType
    {
        Decisions, //standard decision variables (xi)
        Slack, //for <= constraint (si)
        Surplus, //for >= constraint (-ei)
        Atrificial //for >= or = constraint: Two-Phase (ai-ei or ai)
    }

    public enum SignRestriction
    {
        Positive, //(x>=0)
        Negative, //(x<=0)
        Unrestricted //(x is urs)
    }

    public class Variable
    {
        public string Name { get; set; }
        public VariableType Type { get; set; }
        public SignRestriction Restriction { get; set; }
        public double LowerBound { get; set; } = 0.0;
        public double UpperBound { get; set; } = double.PositiveInfinity;

        public Variable(string name, VariableType type = VariableType.Decisions, SignRestriction restriction = SignRestriction.Positive)
        {
            Name = name;
            Type = type;
            Restriction = restriction;
        }
    }
}