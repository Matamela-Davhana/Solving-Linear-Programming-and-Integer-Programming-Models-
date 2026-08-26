using System;
using LinearProgrammingSolver.App.IO;
using LinearProgrammingSolver.Core.Algorithms;
using LinearProgrammingSolver.Core.Models;
using LinearProgrammingSolver.Core.Results;
using LinearProgrammingSolver.Core.Sensitivity;

namespace LinearProgrammingSolver.App
{
    class Program
    {
        static LinearProgram currentModel = null;
        static OutputWriter writer = new OutputWriter(@"C:\Temp\output.txt");
        static SolverResult globalResult = null;
        static void Main(string[] args)
        {
            bool exit = false;

            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("==================================================");
                Console.WriteLine("       LP / IP Solver - Project LPR381            ");
                Console.WriteLine("==================================================");
                Console.WriteLine($"Current Model Loaded: {(currentModel != null ? "YES" : "NO")}");
                Console.WriteLine("==================================================");
                Console.WriteLine("1. Load Programming Model from Text File");
                Console.WriteLine("2. Solve: Primal Simplex");
                Console.WriteLine("3. Solve: Revised Primal Simplex");
                Console.WriteLine("4. Solve: Branch & Bound Simplex");
                Console.WriteLine("5. Solve: Branch & Bound Knapsack");
                Console.WriteLine("6. Solve: Cutting Plane");
                Console.WriteLine("7. Sensitivity Analysis & Duality");
                Console.WriteLine("8. Exit");
                Console.WriteLine("==================================================");
                Console.Write("Select an option (1-8): ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        LoadModel();
                        break;
                    case "2":
                        if (CheckModelLoaded())
                        {
                            Console.WriteLine("\n[Running Primal Simplex...]");
                            writer.ClearPreviousOutput();
                            PrimalSimplexSolver primalSolver = new PrimalSimplexSolver(writer);
                            globalResult = primalSolver.Solve(currentModel);
                            Console.WriteLine($"\nSolve Complete! Status: {globalResult.Status}. Check output.txt");
                        }
                        Pause();
                        break;
                    case "3":
                        if (CheckModelLoaded())
                        {
                            Console.WriteLine("\n[Running Revised Primal Simplex...]");
                            writer.ClearPreviousOutput();
                            RevisedSimplexSolver revisedSolver = new RevisedSimplexSolver(writer);
                            SolverResult result = revisedSolver.Solve(currentModel);
                            Console.WriteLine($"\nSolve Complete! Status: {result.Status}. Check output.txt");
                        }
                        Pause();
                        break;
                    case "4":
                    case "5":
                    case "6":
                        Console.WriteLine("\n[Algorithm pending implementation by Group Members 2 & 3]");
                        Pause();
                        break;
                    case "7":
                        SensitivityAnalysisMenu();
                        break;
                    case "8":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("\nInvalid selection.");
                        Pause();
                        break;
                }
            }
        }

        static void LoadModel()
        {
            Console.Write("Enter the exact file path (e.g., C:\\Temp\\input.txt): ");
            string filePath = Console.ReadLine();

            InputParser parser = new InputParser();
            currentModel = parser.ParseFile(filePath);

            if (currentModel != null)
            {
                Console.WriteLine("\nModel successfully mapped to memory!");
            }
            else
            {
                Console.WriteLine("\nError: Could not load or parse the file. Please check the path and try again.");
            }

            Pause();
        }

        static bool CheckModelLoaded()
        {
            if (currentModel == null)
            {
                Console.WriteLine("\nError: You must load a model (Option 1) first!");
                return false;
            }
            return true;
        }

        static void SensitivityAnalysisMenu()
        {
            // Guard Clause: Ensure we actually have optimal math data to work with
            if (globalResult == null || globalResult.Status != SolutionStatus.Optimal)
            {
                Console.WriteLine("\nError: You must run a Simplex algorithm and find an Optimal solution first!");
                Pause();
                return;
            }

            // Extract the REAL data generated by Simplex engine
            double[] realObjectiveRow = globalResult.OptimalObjectiveRow;
            double[,] realInverseBasis = globalResult.InverseBasis;
            double[,] realFinalTableau = globalResult.FinalTableau;

            SensitivityAnalyzer sensAnalyzer = new SensitivityAnalyzer();
            DualityAnalyzer dualAnalyzer = new DualityAnalyzer();

            bool back = false;
            while (!back)
            {
                Console.Clear();
                Console.WriteLine("==================================================");
                Console.WriteLine("           Sensitivity Analysis & Duality         ");
                Console.WriteLine("==================================================");
                Console.WriteLine("1. Range / Change: Non-Basic Variable");
                Console.WriteLine("2. Range / Change: Basic Variable");
                Console.WriteLine("3. Range / Change: Constraint RHS");
                Console.WriteLine("4. Shadow Prices");
                Console.WriteLine("5. Add New Activity (Variable)");
                Console.WriteLine("6. Add New Constraint");
                Console.WriteLine("7. Duality (Construct, Solve, Verify)");
                Console.WriteLine("8. Return to Main Menu");
                Console.WriteLine("==================================================");
                Console.Write("Select an operation (1-8): ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.Write("Enter Non-Basic Variable Index (e.g., 1 for X1): ");
                        if (int.TryParse(Console.ReadLine(), out int nbVarIndex))
                        {
                            sensAnalyzer.DisplayNonBasicVariableRange(nbVarIndex, realObjectiveRow, realFinalTableau);
                            Console.Write("Enter new coefficient value to apply change: ");
                            if (double.TryParse(Console.ReadLine(), out double nbNewVal))
                                sensAnalyzer.ApplyNonBasicVariableChange(nbVarIndex, nbNewVal);
                        }
                        Pause();
                        break;
                    case "2":
                        Console.Write("Enter Basic Variable Index: ");
                        if (int.TryParse(Console.ReadLine(), out int bVarIndex))
                        {
                            sensAnalyzer.DisplayBasicVariableRange(bVarIndex, realObjectiveRow, realFinalTableau);
                            Console.Write("Enter new coefficient value to apply change: ");
                            if (double.TryParse(Console.ReadLine(), out double bNewVal))
                                sensAnalyzer.ApplyBasicVariableChange(bVarIndex, bNewVal);
                        }
                        Pause();
                        break;
                    case "3":
                        Console.Write("Enter Constraint Index: ");
                        if (int.TryParse(Console.ReadLine(), out int constraintIndex))
                        {
                            double[] originalRHS = new double[currentModel.Constraints.Count];
                            for (int i = 0; i < currentModel.Constraints.Count; i++)
                            {
                                originalRHS[i] = currentModel.Constraints[i].RHS;
                            }

                            sensAnalyzer.DisplayRHSRange(constraintIndex, originalRHS, realInverseBasis);
                            Console.Write("Enter new RHS value to apply change: ");
                            if (double.TryParse(Console.ReadLine(), out double newRHS))
                                sensAnalyzer.ApplyRHSChange(constraintIndex, newRHS, originalRHS, realInverseBasis);
                        }
                        Pause();
                        break;
                    case "4":
                        sensAnalyzer.DisplayShadowPrices(realObjectiveRow);
                        Pause();
                        break;
                    case "5":
                        Console.WriteLine("Capturing new activity data...");
                        sensAnalyzer.AddNewActivity(new double[] { 1, 2, 3 }, 50);
                        Pause();
                        break;
                    case "6":
                        Console.WriteLine("Capturing new constraint data...");
                        sensAnalyzer.AddNewConstraint(new double[] { 1, 2, 3 }, "<=", 100);
                        Pause();
                        break;
                    case "7":
                        //Construct the Dual
                        LinearProgram dualModel = dualAnalyzer.ConstructDualModel(currentModel);
                        //Solve the Dual using Simplex Engine
                        Console.WriteLine("\n[Solving Dual Model...]");
                        PrimalSimplexSolver dualSolver = new PrimalSimplexSolver(writer);
                        SolverResult dualResult = dualSolver.Solve(dualModel);
                        //Compare the real primal objective value against the dual to verify Duality
                        if (dualResult != null && dualResult.Status == SolutionStatus.Optimal)
                        {
                            dualAnalyzer.VerifyDuality(globalResult.OptimalValue, dualResult.OptimalValue);
                        }
                        else
                        {
                            Console.WriteLine("Could not solve the Dual model optimally.");
                        }
                        Pause();
                        break;
                    case "8":
                        back = true;
                        break;
                    default:
                        Console.WriteLine("Invalid selection.");
                        Pause();
                        break;
                }
            }
        }

        static void Pause()
        {
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }
}