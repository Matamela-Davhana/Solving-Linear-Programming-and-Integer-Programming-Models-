using System;
using System.Collections.Generic;
using LinearProgrammingSolver.Core.Algorithms;
using LinearProgrammingSolver.Core.Models;

namespace BranchAndBoundKnapsack
{
    public class BnBKnapsack
    {
        //constant
        private const double EPSILON = 1e-9;

//input here        //result
        public class Result
        {
            public bool IsOptimal { get; set; }
            public double OptimalProfit { get; set; }
            public double TotalWeight { get; set; }
            public bool[] Selected { get; set; }
            public int[] SelectedItemIndex { get; set; }
            public int SubProblemsCreated { get; set; }
            public int SubProblemsDone { get; set; }
            public int SubProblemsBranched { get; set; }
            public SubProblem? BestSubProblem { get; set; }

            public Result()
            {
                Selected = Array.Empty<bool>();
                SelectedItemIndex = Array.Empty<int>();
            }
        }

        //sort items
        private class SortedItems
        {
            public Item Item { get; set; } = null!;
            public int OriginalIndex { get; set; }
        }

        //private
        private List<Item> _items = new List<Item>();
        private double _capacity;
        private double _bestWeight;
        private double _bestProfit;
        private List<int> _bestItems = new List<int>();
        private readonly List<SubProblem> _allSubProblems = new List<SubProblem>();

        //public
        public IReadOnlyList<SubProblem> AllSubProblems => _allSubProblems;
        public SubProblem? BestSubProblem { get; private set; }

        public BnBKnapsack(List<Item> items, double capacity)
        {
            ValidateInput(items, capacity);
            _items = new List<items>(items);
            _capacity = capacity;
        }

        //solve
        public Result Solve()
        {
            //reset
            _allSubProblems.Clear();
            _bestItems.Clear();
            _bestWeight = 0.0;
            _bestProfit = 0.0;
            BestSubProblem = null;

            //ratio order
            var sortedItems = _items.Select((items, index) => new SortedItem
            {
                Item = items,
                OriginalIndex = index
            })
            .OrderByDescending(x => x.Item.Ratio).ToList();
        }

        //root = 0
        var root = new SubProblem(number: "0", parentNumber: "", level: 0, branchItemIndex: -1, weight: 0.0,
                                  profit: 0.0, selectedItems: new List<int>(), branchDescription: "Root Subproblem");

        root.UpperBound = CalculateUpperBound(root, SortedItems);

        //first
        var stack = new Stack<SubProblem>();
        Stack.Push(root);

        int created = 1;
        int done = 0;
        int branched = 0;

        //main loop
        while(Stack.Count>0)
        {
            var current = Stack.Pop();
            done++;

            if(current.UpperBound<= _bestProfit+EPSILON)
            {
              current.ContinueBranching = true;
              current.BranchReason = "Upper bound <= current";
              _allSubProblems.Add(current);
             branched++;
             continue;
            }
           
            if(current.Level>= sortedItems.Count)
             {
              current.ContinueBranching = true;
              current.BranchReason = "Complete solution";
              UpdateBestSolution(current);
              _allSubProblems.Add(current);
              branched++;
              continue;
             }

          // add sbproblem current
          _allSubProblems.Add(current);

         // branching item
         var item = sortedItems[current.Level];

         // child 1
          string includeNumber;

          if (current.Number == "0")
          {
             includeNumber = "1";
          }
          else
          {
             includeNumber = current.Number + ".1";
          }

          double includeWeight = current.Weight + item.Item.Weight;
          double includeProfit = current.Profit + item.Item.Profit;
          var includeItems = new List<int>(current.SelectedItems);
          includeItems.Add(item.OriginalIndex);

          var includeSubProblem =
           new SubProblem(
            number: includeNumber,
            parentNumber: current.Number,
            level: current.Level + 1,
            branchItemIndex: item.OriginalIndex,
            weight: includeWeight,
            profit: includeProfit,
            selectedItems: includeItems,
            branchDescription: $"Include {item.Item.Name} " + $"(x{item.OriginalIndex + 1} = 1)"
            );

            created++;

          // capacity check
          if (includeWeight >_capacity + EPSILON)
          {
              includeSubProblem.ContinueBranching = true;
              includeSubProblem.BranchReason = "Capacity exceeded";
              includeSubProblem.UpperBound = double.NegativeInfinity;
              _allSubProblems.Add(includeSubProblem);
              branched++;
          }
         
          else
          {
              UpdateBestSolution(includeSubProblem);

           //calculate upper bound
           includeSubProblem.UpperBound = CalculateUpperBound(includeSubProblem, sortedItems);

           //branch
           if (includeSubProblem.UpperBound >_bestProfit + EPSILON)
           {
              stack.Push(includeSubProblem);
           }
           else
           {
              includeSubProblem.ContinueBranching = true;
              includeSubProblem.BranchReason = "Upper bound <= current";
              _allSubProblems.Add(includeSubProblem);
              branched++;
           }
          }

        //child 2
        string excludeNumber;

        if (current.Number == "0")
        {
            excludeNumber = "2";
        }
        else
        {
            excludeNumber =  current.Number + ".2";
        }

        var excludeSubProblem =
        new SubProblem(
         number: excludeNumber,
         parentNumber: current.Number,
         level: current.Level + 1,
         branchItemIndex: item.OriginalIndex,
         weight: current.Weight,
         profit: current.Profit,
         selectedItems: new List<int>(current.SelectedItems),
         branchDescription: $"Exclude {item.Item.Name} " + $"(x{item.OriginalIndex + 1} = 0)"
         );

        created++;

        excludeSubProblem.UpperBound = CalculateUpperBound(excludeSubProblem, sortedItems);

        if (excludeSubProblem.UpperBound >_bestProfit + EPSILON)
        {
            stack.Push(excludeSubProblem);
        }
        else
        {
            excludeSubProblem.ContinueBranching = true;
            excludeSubProblem.BranchReason = "Upper bound <= current";
            _allSubProblems.Add(excludeSubProblem);
            branched++;
        }
      }

        //final result
       var selected = new bool[_items.Count];

       foreach (int index in _bestItems)
       {
         selected[index] = true;
       }

       return new Result
       {
          IsOptimal = true,
          OptimalProfit = _bestProfit,
          TotalWeight = _bestWeight,
          Selected = selected,
          SelectedItemIndex = _bestItems.ToArray(),
          SubProblemsCreated = created,
          SubProblemsDone = done,
          SubProblemsBranched = branched,
          BestSubProblem = BestSubProblem
       };
     }

     //calculate next subproblem
        private double CalculateUpperBound(SubProblem subProblem, List<SortedItem> sortedItems)
        {
           if (subProblem.Weight >_capacity + EPSILON)
           {
               return double.NegativeInfinity;
           }

           double bound = subProblem.Profit;
           double remainingCapacity = _capacity - subProblem.Weight;

           //next item
           for (int i = subProblem.Level; i < sortedItems.Count; i++)
           {
              var item = sortedItems[i].Item;

              //no weight
              if (item.Weight <= EPSILON)
              {
                 bound += item.Profit;
                 continue;
              }

              //fits?
              if (item.Weight <= remainingCapacity + EPSILON)
              {
                 remainingCapacity -= item.Weight;
                 bound += item.Profit;
              }
              
              else
              {
                 //fraction fits
                 bound += item.Profit * (remainingCapacity / item.Weight);
                 break;
              }
           }

           return bound;
        }

     //update best solution
     private void UpdateBestSolution(SubProblem subProblem)
     {
        if (subProblem.Weight > _capacity + EPSILON)
        {
           return;
        }

        if (subProblem.Profit > _bestProfit + EPSILON)
        {
           _bestProfit = subProblem.Profit;
           _bestWeight = subProblem.Weight;
           _bestItems = new List<int>(subProblem.SelectedItems);
           BestSubProblem = subProblem;
        }
     }

     //validate input
     private void ValidateInput(List<Item> items, double capacity)
     {
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        if (capacity < 0)
        {
           throw new ArgumentException("Knapsack capacity cannot be negative.");
        }

        foreach (var item in items)
        {
            if (item == null)
            {
               throw new ArgumentException("Item cannot be null.");
            }

           if (item.Weight < 0)
           {
              throw new ArgumentException($"Item '{item.Name}' " + "has negative weight.");
           }

           if (item.Profit < 0)
           {
               throw new ArgumentException($"Item '{item.Name}' " + "has negative profit.");
           }
        }
     }

     //display
     public void PrintResult(Result result)
     {
         Console.WriteLine();
         Console.WriteLine("============================================================");
         Console.WriteLine("              BRANCH AND BOUND KNAPSACK");
         Console.WriteLine("============================================================");
         Console.WriteLine($"Status                : " + $"{(result.IsOptimal ? "Optimal" : "Not Optimal")}");
         Console.WriteLine($"Optimal Profit        : " + $"{result.OptimalProfit:F2}");
         Console.WriteLine($"Total Weight          : " + $"{result.TotalWeight:F2}");
         Console.WriteLine($"Capacity              : " + $"{_capacity:F2}");
         Console.WriteLine($"Subproblems Created : " + $"{result.SubProblemsCreated}");
         Console.WriteLine($"Subproblems Done  : " + $"{result.SubProblemsDone}");
         Console.WriteLine($"Subproblems Branched  : " + $"{result.SubProblemsBranched}");

        if (result.BestSubProblem != null)
        {
           Console.WriteLine($"Best Subproblem       : " + $"{result.BestSubProblem.Number}");
        }

       //selected items
       Console.WriteLine();
       Console.WriteLine("Selected Items:");
       Console.WriteLine("------------------------------------------------------------");
       Console.WriteLine(
        $"{"Item",-12}" +
        $"{"Selected",-12}" +
        $"{"Weight",-12}" +
        $"{"Profit",-12}" +
        $"{"Ratio",-12}");
       Console.WriteLine("------------------------------------------------------------");

        for (int i = 0; i < _items.Count; i++)
        {
            string selected = result.Selected[i]? "Yes": "No";
            Console.WriteLine(
            $"{_items[i].Name,-12}" +
            $"{selected,-12}" +
            $"{_items[i].Weight,-12:F2}" +
            $"{_items[i].Profit,-12:F2}" +
            $"{_items[i].Ratio,-12:F2}");
        }

        //subproblems
         Console.WriteLine();
         Console.WriteLine("Subproblems:");
         Console.WriteLine("====================================================================================================");
         Console.WriteLine($"{"Subproblem",-14}" +
           $"{"Parent",-14}" +
           $"{"Level",-8}" +
           $"{"Weight",-12}" +
           $"{"Profit",-12}" +
           $"{"Upper Bound",-15}" +
           $"{"Status",-35}");
        Console.WriteLine("====================================================================================================");

         foreach (var subProblem in GetSubProblemsInTreeOrder())
         {
             string status;

             if (subProblem.ContinueBranching)
             {
                status = "Branched: " + subProblem.BranchReason;
             }
             else
             {
                status = "Branched";
             }

             Console.WriteLine(
              $"{subProblem.Number,-14}" +
              $"{subProblem.ParentNumber,-14}" +
              $"{subProblem.Level,-8}" +
              $"{subProblem.Weight,-12:F2}" +
              $"{subProblem.Profit,-12:F2}" +
              $"{subProblem.UpperBound,-15:F2}" +
              $"{status,-35}");

             Console.WriteLine($"    {subProblem.BranchDescription}");
         }
        
       Console.WriteLine();
       Console.WriteLine("============================================================");
     }

     //sort subproblems for display
     private List<SubProblem> GetSubProblemsInTreeOrder()
     {
        return _allSubProblems.OrderBy(x => x.Number, new SubProblemNumberComparer()).ToList();
     }

     //subprobroblem numbers compared
     private class SubProblemNumberComparer : IComparer<string>
     {
        public int Compare(string? x, string? y)
        {
            if (x == null && y == null)
              return 0;

            if (x == null)
              return -1;

            if (y == null)
              return 1;

           int[] xParts = x.Split('.').Select(int.Parse).ToArray();
           int[] yParts = y.Split('.').Select(int.Parse).ToArray();
           int length = Math.Min(xParts.Length,yParts.Length);

           for (int i = 0; i < length; i++)
           {
               int comparison = xParts[i].CompareTo(yParts[i]);
               if (comparison != 0)
                 return comparison;
           }

           return xParts.Length.CompareTo(yParts.Length);
        }
     }
    }
}
