using System;
using System.IO;
using System.Collections.Generic;
using LinearProgrammingSolver.Core.Models;

namespace LinearProgrammingSolver.App.IO
{
    public class InputParser
    {
        public LinearProgram ParseFile(string filePath)
        {
            if (!File.Exists(filePath)) return null;

            string[] lines = File.ReadAllLines(filePath);
            LinearProgram lp = new LinearProgram();

            // Map Objective
            lp.Objective = ParseObjectiveFunction(lines[0]);

            // Map Constraints
            for (int i = 1; i < lines.Length - 1; i++)
            {
                var constraint = ParseConstraint(lines[i], i);
                if (constraint != null) lp.Constraints.Add(constraint);
            }

            // The Sign Restrictions (Last Line) would map to the variables here

            return lp;
        }

        private ObjectiveFunction ParseObjectiveFunction(string line)
        {
            string[] tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            ObjectiveType type = tokens[0].ToLower() == "max" ? ObjectiveType.Maximize : ObjectiveType.Minimize;

            List<double> coeffs = new List<double>();
            for (int i = 1; i < tokens.Length; i += 2)
            {
                if (i + 1 >= tokens.Length) break;
                if (double.TryParse(tokens[i + 1], out double val))
                {
                    coeffs.Add(tokens[i] == "-" ? -val : val);
                }
            }
            return new ObjectiveFunction(type, coeffs);
        }

        private Constraint ParseConstraint(string line, int index)
        {
            string[] tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 4) return null;

            double rhs = double.Parse(tokens[tokens.Length - 1]);
            string relString = tokens[tokens.Length - 2];

            Relation rel = Relation.Equal;
            if (relString == "<=") rel = Relation.LessThanOrEqual;
            if (relString == ">=") rel = Relation.GreaterThanOrEqual;

            List<double> coeffs = new List<double>();
            for (int i = 0; i < tokens.Length - 2; i += 2)
            {
                if (double.TryParse(tokens[i + 1], out double val))
                {
                    coeffs.Add(tokens[i] == "-" ? -val : val);
                }
            }
            return new Constraint($"C{index}", coeffs, rel, rhs);
        }
    }
}