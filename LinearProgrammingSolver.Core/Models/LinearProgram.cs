using System;
using System.Collections.Generic;

namespace LinearProgrammingSolver.Core.Models
{
    // Automatic handling of standard canonical form conversion
    public class LinearProgram
    {
        public ObjectiveFunction Objective { get; set; }
        public List<Variable> Variables { get; set; } = new List<Variable>();
        public List<Constraint> Constraints { get; set; } = new List<Constraint>();

        public int NumDecisionVariables => Objective?.Coefficients.Count ?? 0;
        public int NumConstraints => Constraints.Count;

        // Converts LP into Canonical Form and populates full variable list required by Simplex
        public void ConvertToCanonicalForm()
        {
            // Populate Decision Variables
            Variables.Clear();
            for (int i = 0; i < NumDecisionVariables; i++)
            {
                Variables.Add(new Variable($"x{i + 1}", VariableType.Decision));
            }

            int slackCount = 1;
            int surplusCount = 1;
            int artificialCount = 1;

            // Process each constraint
            for (int i = 0; i < Constraints.Count; i++)
            {
                var c = Constraints[i];

                // If RHS < 0, flip sign of RHS, all row coefficients, and invert the relation
                if (c.RHS < 0)
                {
                    c.RHS *= -1;
                    for (int j = 0; j < c.Coefficients.Count; j++)
                    {
                        c.Coefficients[j] *= -1;
                    }

                    if (c.Relation == Relation.LessThanOrEqual)
                        c.Relation = Relation.GreaterThanOrEqual;
                    else if (c.Relation == Relation.GreaterThanOrEqual)
                        c.Relation = Relation.LessThanOrEqual;
                }

                // Append slack, surplus, and artificial variables to the model
                if (c.Relation == Relation.LessThanOrEqual)
                {
                    Variables.Add(new Variable($"s{slackCount++}", VariableType.Slack));
                }
                else if (c.Relation == Relation.GreaterThanOrEqual)
                {
                    Variables.Add(new Variable($"e{surplusCount++}", VariableType.Surplus));
                    Variables.Add(new Variable($"a{artificialCount++}", VariableType.Artificial));
                }
                else if (c.Relation == Relation.Equal)
                {
                    Variables.Add(new Variable($"a{artificialCount++}", VariableType.Artificial));
                }
            }
        }

        // Creates a deep copy of the LinearProgram instance
        public LinearProgram Clone()
        {
            var clone = new LinearProgram
            {
                Objective = new ObjectiveFunction(this.Objective.Type, new List<double>(this.Objective.Coefficients)),
                Variables = new List<Variable>(),
                Constraints = new List<Constraint>()
            };

            foreach (var v in this.Variables)
            {
                clone.Variables.Add(new Variable(v.Name, v.Type, v.Restriction)
                {
                    LowerBound = v.LowerBound,
                    UpperBound = v.UpperBound
                });
            }

            foreach (var c in this.Constraints)
            {
                clone.Constraints.Add(new Constraint(
                    c.Name,
                    new List<double>(c.Coefficients),
                    c.Relation,
                    c.RHS
                ));
            }

            return clone;
        }
    }
}