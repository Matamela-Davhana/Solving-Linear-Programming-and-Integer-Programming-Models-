using System;
using LinearProgrammingSolver.App.IO;
using LinearProgrammingSolver.Core.Sensitivity;

namespace LinearProgrammingSolver.App
{
    class Program
    {
        static void Main(string[] args)
        {
            bool exit = false;

            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("==================================================");
                Console.WriteLine("       LP / IP Solver - Project LPR381            ");
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
                        Console.WriteLine("\n[Running Primal Simplex...]");
                        // TODO: Call Role 1's Primal Simplex algorithm
                        Pause();
                        break;
                    case "3":
                        Console.WriteLine("\n[Running Revised Primal Simplex...]");
                        // TODO: Call Role 1's Revised Simplex algorithm
                        Pause();
                        break;
                    case "4":
                        Console.WriteLine("\n[Running Branch & Bound Simplex...]");
                        // TODO: Call Role 2's B&B logic
                        Pause();
                        break;
                    case "5":
                        Console.WriteLine("\n[Running Branch & Bound Knapsack...]");
                        // TODO: Call Role 3's Knapsack logic
                        Pause();
                        break;
                    case "6":
                        Console.WriteLine("\n[Running Cutting Plane...]");
                        // TODO: Call Role 3's Cutting Plane logic
                        Pause();
                        break;
                    case "7":
                        SensitivityAnalysisMenu();
                        break;
                    case "8":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("\nInvalid selection. Please enter a number between 1 and 8.");
                        Pause();
                        break;
                }
            }
        }

        static void LoadModel()
        {
            Console.WriteLine("\n--- Load Input File ---");

            string filePath = "";

#if DEBUG
            // Hardcoded for rapid debugging.
            filePath = @"C:\Temp\input.txt";
            Console.WriteLine($"[DEBUG MODE] Auto-loading test file from: {filePath}");
//#else
            Console.Write("Enter the exact file path (e.g., input.txt): ");
            filePath = Console.ReadLine();
#endif

            Console.WriteLine($"\n[Attempting to load and parse {filePath} ...]");
            InputParser parser = new InputParser();
            parser.ParseFile(filePath);

            Pause();
        }

        static void SensitivityAnalysisMenu()
        {
            SensitivityAnalyzer sensAnalyzer = new SensitivityAnalyzer();
            DualityAnalyzer dualAnalyzer = new DualityAnalyzer();
            double[] dummyObjectiveRow = new double[] { };
            double[][] dummyMatrix = new double[][] { };
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
                            sensAnalyzer.DisplayNonBasicVariableRange(nbVarIndex, dummyObjectiveRow, dummyMatrix);
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
                            sensAnalyzer.DisplayBasicVariableRange(bVarIndex, dummyObjectiveRow, dummyMatrix);
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
                            sensAnalyzer.DisplayRHSRange(constraintIndex, dummyObjectiveRow, dummyMatrix);
                            Console.Write("Enter new RHS value to apply change: ");
                            if (double.TryParse(Console.ReadLine(), out double newRHS))
                                sensAnalyzer.ApplyRHSChange(constraintIndex, newRHS);
                        }
                        Pause();
                        break;
                    case "4":
                        sensAnalyzer.DisplayShadowPrices(dummyObjectiveRow);
                        Pause();
                        break;
                    case "5":
                        Console.WriteLine("Capturing new activity data...");
                        // In reality we will loop to get all coefficients but we'll pass dummies for the shell
                        sensAnalyzer.AddNewActivity(new double[] { 1, 2, 3 }, 50);
                        Pause();
                        break;
                    case "6":
                        Console.WriteLine("Capturing new constraint data...");
                        sensAnalyzer.AddNewConstraint(new double[] { 1, 2, 3 }, "<=", 100);
                        Pause();
                        break;
                    case "7":
                        object dualModel = dualAnalyzer.ConstructDualModel(null); // Passes dummy null for now
                        dualAnalyzer.SolveDualModel(dualModel);
                        dualAnalyzer.VerifyDuality(150.5, 150.5); // Hardcoded test values
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