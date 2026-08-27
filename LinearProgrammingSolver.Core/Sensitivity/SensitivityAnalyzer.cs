using System;
using System.Diagnostics;

namespace LinearProgrammingSolver.Core.Sensitivity
{
    public class SensitivityAnalyzer
    {
        // Puleng: 
        // You must pass your final Optimal Tableau and the Original LP Model to these 
        // methods. Do NOT change these method signatures. Adapt your SolverResult to fit.

        // Display the range of a selected Non-Basic Variable
        public void DisplayNonBasicVariableRange(int variableIndex, double[] optimalObjectiveRow, double[,] finalTableau)
        {
            Console.WriteLine($"\n--- Range for Non-Basic Variable X{variableIndex} ---");
            int j = variableIndex - 1;

            if (j < 0 || j >= optimalObjectiveRow.Length)
            {
                Console.WriteLine("Error: Invalid index.");
                return;
            }

            // For non-basic variables, the reduced cost in the Z-Row is exactly the allowable change
            double reducedCost = optimalObjectiveRow[j];

            Console.WriteLine($"Current Reduced Cost (Z-Row Penalty): {reducedCost.ToString("F3")}");
            Console.WriteLine($"To become profitable and enter the basis, the coefficient must improve by more than {Math.Abs(reducedCost).ToString("F3")}");
            Console.WriteLine("Allowable Degradation: Infinity (Making it worse won't change the optimal solution)");
        }

        // Apply and display a change of a selected Non-Basic Variable
        public void ApplyNonBasicVariableChange(int variableIndex, double newValue)
        {
            Console.WriteLine($"\n--- Applying Change to Non-Basic Variable X{variableIndex} -> {newValue} ---");
            Console.WriteLine("Status: Change recorded.");
            Console.WriteLine("Mathematical Result: If this new value overcomes the reduced cost penalty, the current basis is no longer optimal, and Primal Simplex must be triggered to pivot this variable in.");
        }

        // Display the range of a selected Basic Variable
        public void DisplayBasicVariableRange(int variableIndex, double[] optimalObjectiveRow, double[,] finalTableau)
        {
            Console.WriteLine($"\n--- Range for Basic Variable X{variableIndex} ---");

            //Added to verify that the matrix and array exist to prevent runtime NullReferenceException
            if(finalTableau == null || optimalObjectiveRow == null)
            {
                Console.WriteLine("Error: Null matrix or objective row provided.");
                return;
            }

            int targetVar = variableIndex - 1;

            int rows = finalTableau.GetLength(0);
            int cols = finalTableau.GetLength(1);

            //Added: Check that requested variable index falls withing matrix bound
            if(targetVar <0 || targetVar >= cols - 1)
            {
                Console.WriteLine($"Error: Variable index X{variableIndex} is out of bounds.");
                return;
            }

            // Find which tableau row this basic variable is sitting in
            int basicRow = -1;
            for (int i = 0; i < rows; i++)
            {
                if (Math.Abs(finalTableau[i, targetVar] - 1.0) < 0.0001)
                {
                    basicRow = i;
                    break;
                }
            }
            //when variabe is non-basic
            if (basicRow == -1)
            {
                Console.WriteLine("Error: This variable is NOT in the current basis.");
                return;
            }

            // Calculate the limits (Delta C) using ratio tests across non-basic columns
            double lowerBoundDelta = double.NegativeInfinity;
            double upperBoundDelta = double.PositiveInfinity;

            for (int j = 0; j < cols - 1; j++)
            {
                if (j < optimalObjectiveRow.Length && optimalObjectiveRow[j] > 0.0001) // If it is a non-basic column
                {
                    double y_ij = finalTableau[basicRow, j]; // The matrix element
                    if (Math.Abs(y_ij) > 0.00001)
                    {
                        double ratio = -optimalObjectiveRow[j] / y_ij;

                        if (y_ij > 0)
                        {
                            if (ratio > lowerBoundDelta) lowerBoundDelta = ratio;
                        }
                        else
                        {
                            if (ratio < upperBoundDelta) upperBoundDelta = ratio;
                        }
                    }
                }
            }

            string decStr = lowerBoundDelta == double.NegativeInfinity ? "-Infinity" : lowerBoundDelta.ToString("F3");
            string incStr = upperBoundDelta == double.PositiveInfinity ? "+Infinity" : upperBoundDelta.ToString("F3");

            Console.WriteLine($"Variable found in Tableau Row: {basicRow}");
            Console.WriteLine($"Allowable Coefficient Change (Delta C): [ {decStr} , {incStr} ]");
            Console.WriteLine("As long as the change to the objective coefficient stays within these limits, the current variables remain the optimal choices.");
        }

        // Apply and display a change of a selected Basic Variable
        public void ApplyBasicVariableChange(int variableIndex, double newValue)
        {
            Console.WriteLine($"\n--- Applying Change to Basic Variable X{variableIndex} -> {newValue} ---");
            Console.WriteLine("Status: Change recorded.");
            Console.WriteLine("Mathematical Result: The Z-Row must be recalculated using the new coefficient (c_B * B^-1). If any non-basic variable's reduced cost drops below zero, the Simplex algorithm must resume to find the new optimal basis.");
        }

        // Display the range of a selected constraint right-hand-side (RHS) value
        public void DisplayRHSRange(int constraintIndex, double[] originalRHS, double[,] inverseBasis)
        {
            Console.WriteLine($"\n--- Range for Constraint {constraintIndex} RHS ---");

            // Convert human-readable index (e.g., Constraint 1) to 0-based array index
            int i = constraintIndex - 1;

            if (i < 0 || i >= originalRHS.Length)
            {
                Console.WriteLine("Error: Invalid constraint index.");
                return;
            }

            int numConstraints = originalRHS.Length;

            // Calculate current basic solution X_B = B^-1 * b
            double[] x_B = new double[numConstraints];
            for (int row = 0; row < numConstraints; row++)
            {
                for (int col = 0; col < numConstraints; col++)
                {
                    x_B[row] += inverseBasis[row, col] * originalRHS[col];
                }
            }

            // Find limits for the change (Delta b)
            double maxDecrease = double.NegativeInfinity; // Highest lower bound
            double maxIncrease = double.PositiveInfinity; // Lowest upper bound

            for (int row = 0; row < numConstraints; row++)
            {
                // alpha is the element in the Inverse Basis corresponding to the constraint
                double alpha = inverseBasis[row, i];

                if (Math.Abs(alpha) > 0.00001) // Prevent division by zero
                {
                    double limit = -x_B[row] / alpha;

                    if (alpha > 0)
                    {
                        // alpha > 0 implies delta b >= -X_B / alpha
                        if (limit > maxDecrease) maxDecrease = limit;
                    }
                    else
                    {
                        // alpha < 0 implies delta b <= -X_B / alpha
                        if (limit < maxIncrease) maxIncrease = limit;
                    }
                }
            }

            // Apply the allowable changes to the original RHS
            double lowerBound = originalRHS[i] + maxDecrease;
            double upperBound = originalRHS[i] + maxIncrease;

            string lowerStr = maxDecrease == double.NegativeInfinity ? "-Infinity" : lowerBound.ToString("F3");
            string upperStr = maxIncrease == double.PositiveInfinity ? "+Infinity" : upperBound.ToString("F3");

            Console.WriteLine($"Original RHS: {originalRHS[i].ToString("F3")}");
            Console.WriteLine($"Allowable Range: [ {lowerStr} , {upperStr} ]");
        }

        // Apply and display a change of a selected constraint RHS value
        public void ApplyRHSChange(int constraintIndex, double newRHSValue, double[] originalRHS, double[,] inverseBasis)
        {
            Console.WriteLine($"\n--- Applying Change to Constraint {constraintIndex} RHS -> {newRHSValue} ---");

            int i = constraintIndex - 1;
            if (i < 0 || i >= originalRHS.Length)
            {
                Console.WriteLine("Error: Invalid constraint index.");
                return;
            }

            int numConstraints = originalRHS.Length;

            //Create the new RHS vector (b_new)
            double[] newRHSVector = new double[numConstraints];
            for (int j = 0; j < numConstraints; j++)
            {
                newRHSVector[j] = (j == i) ? newRHSValue : originalRHS[j];
            }

            //Calculate the new Basic Solution (X_B = B^-1 * b_new)
            double[] newX_B = new double[numConstraints];
            bool isFeasible = true;

            Console.WriteLine("\nNew Basic Variable Values (X_B):");
            for (int row = 0; row < numConstraints; row++)
            {
                for (int col = 0; col < numConstraints; col++)
                {
                    newX_B[row] += inverseBasis[row, col] * newRHSVector[col];
                }

                Console.WriteLine($"Basic Variable Row {row + 1}: {newX_B[row].ToString("F3")}");

                // If any basic variable becomes strictly negative, the basis is infeasible
                if (newX_B[row] < -0.00001)
                {
                    isFeasible = false;
                }
            }

            // Evaluate the result
            if (isFeasible)
            {
                Console.WriteLine("\nResult: The current basis remains OPTIMAL and FEASIBLE.");
                Console.WriteLine("The change falls within the allowable range.");
            }
            else
            {
                Console.WriteLine("\nResult: The current basis is now INFEASIBLE (Variables dropped below 0).");
                Console.WriteLine("The change is outside the allowable range. A Dual Simplex algorithm is required to find the new optimal basis.");
            }
        }

        // Add a new activity (variable) to an optimal solution
        public void AddNewActivity(double[] newActivityCoefficients, double newObjectiveCoefficient, double[] shadowPrices)
        {
            Console.WriteLine("\n--- Adding New Activity (Variable) ---");
            //(Math Logic): Price out the new activity.If negative(for max), pivot it in.

            if (newActivityCoefficients == null || shadowPrices == null || newActivityCoefficients.Length != shadowPrices.Length)
            {
                Console.WriteLine("Error: Constraint coefficients count must match the number of shadow prices.");
                return;
            }

            // Calculate z_new = sum(shadow_price_i * a_i_new)
            double z_new = 0.0;
            for (int i = 0; i < shadowPrices.Length; i++)
            {
                z_new += shadowPrices[i] * newActivityCoefficients[i];
            }

            // Reduced Cost: c_new - z_new (for Maximization)
            double reducedCost = newObjectiveCoefficient - z_new;

            Console.WriteLine($"New Activity Objective Coefficient (c_new): {newObjectiveCoefficient:F3}");
            Console.WriteLine($"Evaluated Resource Cost (z_new = y * A_new): {z_new:F3}");
            Console.WriteLine($"Calculated Reduced Cost: {reducedCost:F3}");

            if (reducedCost > 0.0001)
            {
                Console.WriteLine("\nResult: The new activity is PROFITABLE (Reduced Cost > 0).");
                Console.WriteLine("Action: Add this column to the LP model and re-run Primal Simplex to pivot this variable into the basis.");
            }
            else
            {
                Console.WriteLine("\nResult: The new activity is NOT profitable (Reduced Cost <= 0).");
                Console.WriteLine("Action: The current optimal solution remains unchanged. The new variable will stay at 0.");
            }
        }

        // Add a new constraint to an optimal solution
        public void AddNewConstraint(double[] newConstraintCoefficients, string relation, double rhs, double[] currentOptimalX)
        {
            Console.WriteLine("\n--- Adding New Constraint ---");
            //(Math Logic): Check if current optimal solution satisfies this. If not, Dual Simplex.

            if (newConstraintCoefficients == null || currentOptimalX == null || newConstraintCoefficients.Length > currentOptimalX.Length)
            {
                Console.WriteLine("Error: Coefficient dimensions do not match the current decision variables.");
                return;
            }

            // Calculate Left-Hand Side (LHS) = sum(a_i * x_i*)
            double lhs = 0.0;
            for (int i = 0; i < newConstraintCoefficients.Length; i++)
            {
                lhs += newConstraintCoefficients[i] * currentOptimalX[i];
            }

            Console.WriteLine($"Evaluated LHS Value: {lhs:F3}");
            Console.WriteLine($"Constraint Condition: {lhs:F3} {relation} {rhs:F3}");

            bool isSatisfied = false;
            switch (relation.Trim())
            {
                case "<=":
                case "≤":
                    isSatisfied = lhs <= rhs + 0.0001;
                    break;
                case ">=":
                case "≥":
                    isSatisfied = lhs >= rhs - 0.0001;
                    break;
                case "=":
                case "==":
                    isSatisfied = Math.Abs(lhs - rhs) < 0.0001;
                    break;
                default:
                    Console.WriteLine($"Error: Unrecognized inequality relation '{relation}'.");
                    return;
            }

            if (isSatisfied)
            {
                Console.WriteLine("\nResult: The current optimal solution SATISFIES the new constraint.");
                Console.WriteLine("Action: The solution remains OPTIMAL and FEASIBLE without recalculation.");
            }
            else
            {
                Console.WriteLine("\nResult: The current optimal solution VIOLATES the new constraint.");
                Console.WriteLine("Action: The current basis is now INFEASIBLE. Add a slack/surplus variable and apply Dual Simplex to restore feasibility.");
            }
        }

        // Display the shadow prices
        public void DisplayShadowPrices(double[] optimalObjectiveRow)
        {
            Console.WriteLine("\n--- Shadow Prices ---");

            if (optimalObjectiveRow == null || optimalObjectiveRow.Length == 0)
            {
                Console.WriteLine("Error: No objective row data found.");
                return;
            }
            int numConstraints = optimalObjectiveRow.Length / 2; // Rough estimate based on standard LP
            int startIndex = Math.Max(0,optimalObjectiveRow.Length - numConstraints); //ensure startIndex is never -
            
            Console.WriteLine("Shadow Prices (Dual Values) for Constraints:");
            for (int i = 0; i < numConstraints; i++)
            {
                int targetIndex = startIndex + i;

                // Check bounds just to be safe
                if (targetIndex + i < optimalObjectiveRow.Length)
                {
                    double shadowPrice = optimalObjectiveRow[startIndex + i];
                    Console.WriteLine($"Constraint {i + 1}: {shadowPrice.ToString("F3")}");
                }
            }
        }
    }
}
