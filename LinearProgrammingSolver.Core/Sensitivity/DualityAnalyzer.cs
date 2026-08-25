using System;
using LinearProgrammingSolver.Core.Models;

namespace LinearProgrammingSolver.Core.Sensitivity
{
    public class DualityAnalyzer
    {
        // Puleng: 
        // This handles the Dual conversion.

        // Apply/Construct Duality to the programming model
        public LinearProgram ConstructDualModel(LinearProgram primal)
        {
            Console.WriteLine("\n--- Constructing Dual Model ---");

            if (primal == null)
            {
                Console.WriteLine("Error: Primal model is null.");
                return null;
            }

            LinearProgram dual = new LinearProgram();

            // Swap Min/Max and use Primal RHS as Dual Objective Coefficients
            ObjectiveType dualType = primal.Objective.Type == ObjectiveType.Maximize
                ? ObjectiveType.Minimize
                : ObjectiveType.Maximize;

            List<double> dualObjCoeffs = new List<double>();
            foreach (var constraint in primal.Constraints)
            {
                dualObjCoeffs.Add(constraint.RHS);
            }
            dual.Objective = new ObjectiveFunction(dualType, dualObjCoeffs);

            // Transpose Matrix: Primal Variables become Dual Constraints
            int numPrimalVars = primal.NumDecisionVariables;
            int numPrimalConstraints = primal.Constraints.Count;

            for (int j = 0; j < numPrimalVars; j++)
            {
                List<double> dualConstraintCoeffs = new List<double>();
                for (int i = 0; i < numPrimalConstraints; i++)
                {
                    // Transpose: Column j of primal becomes Row j of dual
                    dualConstraintCoeffs.Add(primal.Constraints[i].Coefficients[j]);
                }

                // Primal Objective Coefficients become Dual RHS
                double dualRHS = primal.Objective.Coefficients[j];

                // Determine Relation 
                // Standard rule: Primal Max with <= constraints -> Dual Min with >= constraints
                Relation dualRelation = dualType == ObjectiveType.Minimize
                    ? Relation.GreaterThanOrEqual
                    : Relation.LessThanOrEqual;

                dual.Constraints.Add(new Constraint($"Dual_C{j + 1}", dualConstraintCoeffs, dualRelation, dualRHS));
            }

            Console.WriteLine("Dual Model successfully constructed in memory.");
            return dual;
        }

        // Solve the Dual Programming Model
        public void SolveDualModel(object dualLinearProgram)
        {
            Console.WriteLine("\n--- Solving Dual Model ---");
            // TODO: Pass the dual model into Member 1's Simplex algorithm.
        }

        // Verify whether the Programming Model has Strong or Weak Duality
        public void VerifyDuality(double primalObjectiveValue, double dualObjectiveValue)
        {
            Console.WriteLine("\n--- Verifying Duality ---");
            Console.WriteLine($"Primal Z = {primalObjectiveValue}");
            Console.WriteLine($"Dual W = {dualObjectiveValue}");

            if (Math.Abs(primalObjectiveValue - dualObjectiveValue) < 0.0001)
            {
                Console.WriteLine("Result: STRONG Duality verified (Z == W).");
            }
            else
            {
                Console.WriteLine("Result: WEAK Duality verified (Z != W).");
            }
        }
    }
}
