using System;
using System.IO;

namespace LinearProgrammingSolver.App.UI
{
    public class InputParser
    {
        public void ParseFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: File not found at {filePath}");
                return;
            }

            string[] lines = File.ReadAllLines(filePath);

            if (lines.Length < 3)
            {
                Console.WriteLine("Error: Invalid file format.");
                return;
            }

            ParseObjectiveFunction(lines[0]);

            for (int i = 1; i < lines.Length - 1; i++)
            {
                ParseConstraint(lines[i]);
            }

            ParseSignRestrictions(lines[lines.Length - 1]);
        }

        //Objective Function Helper
        private void ParseObjectiveFunction(string line)
        {
            string[] tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 3) return;

            Console.WriteLine($"\n--- Parsing Objective: {tokens[0].ToUpper()} ---");

            for (int i = 1; i < tokens.Length; i += 2)
            {
                if (i + 1 >= tokens.Length) break;

                string sign = tokens[i];
                if (double.TryParse(tokens[i + 1], out double coefficient))
                {
                    if (sign == "-") coefficient *= -1;
                    Console.WriteLine($"Obj Coefficient: {coefficient}");
                }
            }
        }

        //Constraint Helper
        private void ParseConstraint(string line)
        {
            string[] tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 4) return;

            string rhsString = tokens[tokens.Length - 1];
            string relation = tokens[tokens.Length - 2];

            Console.WriteLine($"\n--- Parsing Constraint (Relation: {relation}, RHS: {rhsString}) ---");

            for (int i = 0; i < tokens.Length - 2; i += 2)
            {
                string sign = tokens[i];
                if (double.TryParse(tokens[i + 1], out double coefficient))
                {
                    if (sign == "-") coefficient *= -1;
                    Console.WriteLine($"Constraint Coefficient: {coefficient}");
                }
            }
        }

        //Sign Restrictions Helper
        private void ParseSignRestrictions(string line)
        {
            string[] restrictions = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            Console.WriteLine("\n--- Parsing Sign Restrictions ---");

            for (int i = 0; i < restrictions.Length; i++)
            {
                Console.WriteLine($"Variable {i + 1}: {restrictions[i]}");
            }
        }
    }
}