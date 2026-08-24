using System;

namespace LinearProgrammingSolver.Core.Sensitivity
{
    public class DualityAnalyzer
    {
        // Puleng: 
        // This handles the Dual conversion.

        // Apply/Construct Duality to the programming model
        public object ConstructDualModel(object originalLinearProgram)
        {
            Console.WriteLine("\n--- Constructing Dual Model ---");
            // TODO: Transpose objective coefficients to RHS, RHS to objective.
            // Swap min/max, flip relations based on variable signs.
            // RETURNS: A new LinearProgram object representing the Dual.
            return null;
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
