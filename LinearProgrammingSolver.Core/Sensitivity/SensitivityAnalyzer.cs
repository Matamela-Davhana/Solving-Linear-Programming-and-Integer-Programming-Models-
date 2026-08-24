using System;

namespace LinearProgrammingSolver.Core.Sensitivity
{
    public class SensitivityAnalyzer
    {
        // Puleng: 
        // You must pass your final Optimal Tableau and the Original LP Model to these 
        // methods. Do NOT change these method signatures. Adapt your SolverResult to fit.

        // Display the range of a selected Non-Basic Variable
        public void DisplayNonBasicVariableRange(int variableIndex, double[] optimalObjectiveRow, double[][] constraints)
        {
            Console.WriteLine($"\n--- Range for Non-Basic Variable X{variableIndex} ---");
            // TODO (Math Logic): Calculate upper and lower bounds using the optimal tableau
            Console.WriteLine("// Member 1: Integrate your ratio test logic here.");
        }

        // Apply and display a change of a selected Non-Basic Variable
        public void ApplyNonBasicVariableChange(int variableIndex, double newValue)
        {
            Console.WriteLine($"\n--- Applying Change to Non-Basic Variable X{variableIndex} -> {newValue} ---");
            // TODO (Math Logic): Re-evaluate objective row and print if optimal status changes
        }

        // Display the range of a selected Basic Variable
        public void DisplayBasicVariableRange(int variableIndex, double[] optimalObjectiveRow, double[][] inverseBasis)
        {
            Console.WriteLine($"\n--- Range for Basic Variable X{variableIndex} ---");
            // TODO (Math Logic): Use inverse basis matrix to calculate allowable increase/decrease
        }

        // Apply and display a change of a selected Basic Variable
        public void ApplyBasicVariableChange(int variableIndex, double newValue)
        {
            Console.WriteLine($"\n--- Applying Change to Basic Variable X{variableIndex} -> {newValue} ---");
            // TODO (Math Logic): Re-evaluate and trigger revised simplex if optimality is lost
        }

        // Display the range of a selected constraint right-hand-side (RHS) value
        public void DisplayRHSRange(int constraintIndex, double[] originalRHS, double[][] inverseBasis)
        {
            Console.WriteLine($"\n--- Range for Constraint {constraintIndex} RHS ---");
            // TODO (Math Logic): Calculate feasibility bounds (Dual prices)
        }

        // Apply and display a change of a selected constraint RHS value
        public void ApplyRHSChange(int constraintIndex, double newRHSValue)
        {
            Console.WriteLine($"\n--- Applying Change to Constraint {constraintIndex} RHS -> {newRHSValue} ---");
            // TODO (Math Logic): Update RHS, multiply by inverse basis. If negative, trigger Dual Simplex.
        }

        // Add a new activity (variable) to an optimal solution
        public void AddNewActivity(double[] newActivityCoefficients, double newObjectiveCoefficient)
        {
            Console.WriteLine("\n--- Adding New Activity (Variable) ---");
            // TODO (Math Logic): Price out the new activity. If negative (for max), pivot it in.
        }

        // Add a new constraint to an optimal solution
        public void AddNewConstraint(double[] newConstraintCoefficients, string relation, double rhs)
        {
            Console.WriteLine("\n--- Adding New Constraint ---");
            // TODO (Math Logic): Check if current optimal solution satisfies this. If not, Dual Simplex.
        }

        // Display the shadow prices
        public void DisplayShadowPrices(double[] optimalObjectiveRow)
        {
            Console.WriteLine("\n--- Shadow Prices ---");
            // TODO (Math Logic): Extract shadow prices from the slack/surplus columns of the Z-row.
            Console.WriteLine("// Member 1: Extract from Z-Row and map to constraints.");
        }
    }
}
