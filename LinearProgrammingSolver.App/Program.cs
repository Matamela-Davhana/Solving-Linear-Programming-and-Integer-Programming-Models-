using System;
using LinearProgrammingSolver.App.UI;

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
            bool back = false;
            while (!back)
            {
                Console.Clear();
                Console.WriteLine("==================================================");
                Console.WriteLine("           Sensitivity Analysis & Duality         ");
                Console.WriteLine("==================================================");
                Console.WriteLine("1. Variable Ranges / Changes");
                Console.WriteLine("2. RHS Ranges / Changes");
                Console.WriteLine("3. Shadow Prices");
                Console.WriteLine("4. Add Activity (Variable) / Add Constraint");
                Console.WriteLine("5. Duality (Construct Dual, Solve, Verify)");
                Console.WriteLine("6. Return to Main Menu");
                Console.WriteLine("==================================================");
                Console.Write("Select an operation (1-6): ");

                string choice = Console.ReadLine();

                if (choice == "6")
                {
                    back = true;
                }
                else
                {
                    Console.WriteLine($"\n[Executing Sensitivity Option {choice} ...]");
                    // TODO: Hook up your Sensitivity modules here
                    Pause();
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