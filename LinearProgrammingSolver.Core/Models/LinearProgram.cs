using System;
using System.Collections.Generic;

namespace LinearProgrammingSolver.Core
{
    //automatic handling of standard canonical form coversion
    public class LinearProgram
    {
        public ObjectiveFunction Objective {  get; set; }
        public List<Variable> Variables { get; set; } = new List<Variable>();
        public List<Constraint> Constraints { get; set; } = new List<Constraint> ();

        public int NumDecisionVariables => Objective?.Coefficients.Count ?? 0;
        public int NumConstraints => Constraints.Count;

        //function to convert LP into Canonical Form and prepares the full variable list required by Simplex
        public void ConvertToCanonicalForm()
        {
            //Decision Variables
            Variables.Clear ();
            for (int i = 0; i < NumDecisionVariables; i++) 
            {
                Variables.Add(new Variable($"x{i + 1}", VariableType.Decision));
            }

            int slackCount = 1;
            int surplusCount = 1;
            int artificialCount = 1;

            //Adding slack, surplus, and artificial variables based on relations
            for(int i = 0; i < Constraints.Count; i++)
            {
                var c = Constraints[i];

                //if rhs <0, flip sign to make it >0
                if(c.RHS <0)
                {
                    c.RHS *= -1;
                    for(int j = 0; j < Constraints.Count; j++) c.Coefficients[j] *= -1;
                    if (c.Relation == Relation.LessThanOrEqual) c.Relation = Relation.GreatherThanOrEqual;
                    else if (c.Relation == Relation.GreaterThanOrEqual) c.Relation = Relation.LessThanOrEqual;
                }

                if (c.Relation == Relation.LessThanOrEqual)
                {
                    Variables.Add(new Variable($"s{slackCount++}", VariableType.Slack));
                }
                else if (c.Relation  == Relation.GreaterThanOrEqual)
                {
                    Variables.Add(new Variable($"e{surplusCount++}", VariableType.Surplus));
                    Variables.Add(new Variable($"a{artificialCount++}", VariableType.Artificial));
                }
                else if (c.Relation == Relation.Equal)
                {
                    Variables.Add(new Variable($"a{artificialCount++}", VariableType.Artificial))
                }
            }
    }
}