using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZLibrary.Algorithms
{
    public class SimplexAlgorithm : ACStateAlgorithm<SimplexAlgorithm.Input, SimplexAlgorithm.Output, SimplexAlgorithm.State>
    {
        public struct Input
        {
            public Objective Objective;
            public SimplexDecision[] Decisions;
            public Constraint[] Constraints;
        }
        public struct Output
        {
            public Objective Objective;
            public SimplexDecision[] Decisions;
        }
        public struct State
        {

        }

        protected override void processThread()
        {
        }
        public override Output process(Input input)
        {
            Output output = new Output();

            // Create Variable Table Including The Slack Variables
            SimplexDecision[] vars = new SimplexDecision[input.Decisions.Length + input.Constraints.Length];
            Array.Copy(input.Decisions, 0, vars, 0, input.Decisions.Length);
            int i = input.Decisions.Length;

            SimplexMatrix matrix = new SimplexMatrix(input.Decisions.Length, input.Constraints.Length);



            return output;
        }


    }

    public class SimplexMatrix
    {
        public decimal[,] values;
        public decimal[] variables;
        public int[] slacks;

        public IEnumerable<decimal> ObjectiveCoefficients
        {
            get
            {
                for (int i = 0; i < VariableCount; i++)
                {
                    yield return this[i, OBJ];
                }
            }
        }
        public bool IsOptimalMin
        {
            get
            {
                foreach (var d in ObjectiveCoefficients) { if (d < 0) { return false; } }
                return true;
            }
        }
        public bool IsOptimalMax
        {
            get
            {
                foreach (var d in ObjectiveCoefficients) { if (d > 0) { return false; } }
                return true;
            }
        }

        public int DecisionVariableCount { get; protected set; }
        public int SlackVariableCount { get { return ConstraintsCount; } set { ConstraintsCount = value; } }
        public int VariableCount { get { return SlackVariableCount + DecisionVariableCount; } }
        public int RHS { get { return VariableCount; } }

        public int ConstraintsCount { get; protected set; }
        public int OBJ { get { return ConstraintsCount; } }

        public decimal this[int x, int y]
        {
            get { return values[x, y]; }
            set { values[x, y] = value; }
        }
        public decimal this[int var]
        {
            get { return variables[var]; }
            set { variables[var] = value; }
        }

        public SimplexMatrix(int var, int constraints)
        {
            /* Matrix View:
             *  
             * [Constraint <= Value] -> [Constraint + Slack == Value, Slack >= 0]
             * [Constraint >= Value] -> [Constraint - Slack == Value, Slack >= 0]
             * 
             *  ________________ _
             * |S|              |R|
             * |I| Coefficients |H|
             * |_|______________|S|
             *   |____Values____|_| -> Obj
             * 
             */

            DecisionVariableCount = var;
            ConstraintsCount = constraints;

            values = new decimal[
                VariableCount + 1,
                ConstraintsCount + 1
                ];
            variables = new decimal[VariableCount];
            slacks = new int[SlackVariableCount];
            for (int i = 0; i < SlackVariableCount; i++)
            { slacks[i] = i + DecisionVariableCount; }
        }

        public void solveMinimization()
        {
            while (!IsOptimalMin)
            {
                // Find The Largest Contribution
                int largestContributor = 0;
                for (int i = 1; i < VariableCount; i++)
                {
                    if (this[i, OBJ] < this[largestContributor, OBJ] && !slacks.Contains(i))
                    { largestContributor = i; }
                }

                // Calculate Where It Is The Most Effective
                decimal?[] ratios = new decimal?[ConstraintsCount];
                int smallestCI = -1;
                for (int i = 0; i < ConstraintsCount; i++)
                {
                    if (this[largestContributor, i] == 0) { ratios[i] = null; }
                    else
                    {
                        ratios[i] = this[RHS, i] / this[largestContributor, i];
                        if (ratios[i] > decimal.Zero && (smallestCI == -1 || ratios[i] < ratios[smallestCI])) { smallestCI = i; }
                    }
                }
                if (smallestCI == -1) { this[largestContributor, OBJ] = 0m; }
                else
                {
                    // Swap The Values
                    slacks[smallestCI] = largestContributor;

                    // Divide All Values On Constraint By The Coefficient
                    decimal co = this[largestContributor, smallestCI];
                    for (int i = 0; i <= RHS; i++)
                    {
                        this[i, smallestCI] /= co;
                    }

                    // Subtract Middle Values And Add Outer
                    for (int ci = 0; ci < RHS; ci++)
                    {
                        if (ci == largestContributor) { continue; }
                        for (int ri = 0; ri <= OBJ; ri++)
                        {
                            if (ri == smallestCI) { continue; }
                            this[ci, ri] -= this[largestContributor, ri] * this[ci, smallestCI];
                        }
                    }
                    for (int ri = 0; ri < OBJ; ri++)
                    {
                        if (ri == smallestCI) { continue; }
                        this[RHS, ri] -= this[largestContributor, ri] * this[RHS, smallestCI];
                    }
                    this[RHS, OBJ] += this[largestContributor, OBJ] * this[RHS, smallestCI];

                    // Set The Column
                    for (int ri = 0; ri <= OBJ; ri++)
                    {
                        if (ri == smallestCI) { this[largestContributor, ri] = decimal.One; }
                        else { this[largestContributor, ri] = decimal.Zero; }
                    }
                }
            }
        }
    }

    

    #region Helpers
    public interface ISimplexArg
    {
        string Name { get; }
        decimal Value { get; }
    }
    public class SimplexDecision : ISimplexArg
    {
        public static SimplexDecision None { get { return new SimplexDecision(null); } }

        protected string name;
        public string Name
        {
            get { return name; }
            protected set { name = value; }
        }
        protected decimal num;
        public decimal? UpperBound, LowerBound;
        public decimal Value
        {
            get { return num; }
            set
            {
                num = KeepInteger ? System.Math.Round(value) : value;
            }
        }
        public bool KeepInteger;
        public bool IsNone
        {
            get { return string.IsNullOrEmpty(Name); }
        }

        public SimplexDecision(string name, bool keepInt = false, decimal start = 0.0m)
        {
            Name = name;
            KeepInteger = keepInt;
            Value = start;
            UpperBound = null;
            LowerBound = null;
        }
        public void boundConstrain()
        {
            if (UpperBound.HasValue) { if (num > UpperBound) { num = KeepInteger ? System.Math.Floor(UpperBound.Value) : UpperBound.Value; } }
            else if (LowerBound.HasValue) { if (num < LowerBound) { Value = KeepInteger ? System.Math.Ceiling(LowerBound.Value) : LowerBound.Value; } }
        }
    }
    public class SimplexConstant : ISimplexArg
    {
        public static SimplexConstant Zero { get { return new SimplexConstant("Zero", 0m); } }
        public static SimplexConstant One { get { return new SimplexConstant("One", 1m); } }

        protected string name;
        public string Name
        {
            get { return name; }
            protected set { name = value; }
        }
        protected decimal num;
        public decimal Value
        {
            get { return num; }
            protected set { num = value; }
        }

        protected SimplexConstant(string name, decimal value)
        {
            Name = name;
            Value = value;
        }
        public static SimplexConstant newReal(string name, decimal value)
        {
            return new SimplexConstant(name, value);
        }
        public static SimplexConstant newInt(string name, decimal value)
        {
            return new SimplexConstant(name, System.Math.Round(value));
        }
    }
    public struct ConstBind : ISimplexArg
    {
        public SimplexDecision Decision;
        public SimplexConstant Constant;

        public decimal Value { get { return Decision.IsNone ? Constant.Value : Constant.Value * Decision.Value; } }
        public string Name { get { return Decision.IsNone ? Constant.Name : Decision.Name; } }

        public ConstBind(SimplexConstant c, SimplexDecision d)
        {
            Decision = d;
            Constant = c;
        }
        public ConstBind(SimplexDecision d) : this(SimplexConstant.One, d) { }
        public ConstBind(SimplexConstant c) : this(c, SimplexDecision.None) { }
    }
    public enum ConstraintCheck : byte
    {
        LessThan = Flags.Bit1,
        GreaterThan = Flags.Bit2,
        EqualTo = Flags.Bit3,

        Invalid = LessThan | GreaterThan,
        None = Flags.NoFlags,

        Minimize = LessThan,
        Maximize = GreaterThan
    }
    public class Constraint
    {
        public ConstBind[] LeftSide;
        public decimal LeftValue
        {
            get
            {
                decimal d = 0m;
                foreach (var a in LeftSide)
                {
                    d += a.Value;
                }
                return d;
            }
        }
        public SimplexConstant RightSide;
        public ConstraintCheck OpCheck;
        public ConstraintCheck Inverted
        {
            get
            {
                if (OpCheck.HasFlag(ConstraintCheck.LessThan))
                {
                    return OpCheck.HasFlag(ConstraintCheck.EqualTo)
                        ? ConstraintCheck.GreaterThan | ConstraintCheck.EqualTo
                        : ConstraintCheck.GreaterThan;
                }
                else if (OpCheck.HasFlag(ConstraintCheck.GreaterThan))
                {
                    return OpCheck.HasFlag(ConstraintCheck.EqualTo)
                        ? ConstraintCheck.LessThan | ConstraintCheck.EqualTo
                        : ConstraintCheck.LessThan;
                }
                return ConstraintCheck.Invalid;
            }
        }

        public Constraint(ConstraintCheck check, SimplexConstant right, params ConstBind[] left)
        {
            RightSide = right;
            LeftSide = left;
            OpCheck = check;
        }

        public bool IsValid
        {
            get
            {
                if (OpCheck == ConstraintCheck.Invalid || OpCheck == ConstraintCheck.None) { return false; }
                if (OpCheck.HasFlag(ConstraintCheck.GreaterThan))
                { return OpCheck.HasFlag(ConstraintCheck.EqualTo) ? LeftValue >= RightSide.Value : LeftValue > RightSide.Value; }
                else if (OpCheck.HasFlag(ConstraintCheck.LessThan))
                { return OpCheck.HasFlag(ConstraintCheck.EqualTo) ? LeftValue <= RightSide.Value : LeftValue < RightSide.Value; }
                throw new NotImplementedException("Not A Valid Check");
            }
        }
    }
    public class Objective
    {
        public ConstBind[] Summation;
        public IEnumerable<ConstBind> DecisionBinds
        {
            get
            {
                foreach (var s in Summation)
                {
                    if (!s.Decision.IsNone) { yield return s; }
                }
            }
        }
        public IEnumerable<ConstBind> Constants
        {
            get
            {
                foreach (var s in Summation)
                {
                    if (s.Decision.IsNone) { yield return s; }
                }
            }
        }
        public ConstraintCheck OpCheck;

        public Objective(ConstraintCheck type, params ConstBind[] args)
        {
            OpCheck = type;
            Summation = args;
        }
        public Objective(Objective other, IEnumerable<ConstBind> args)
        {
            OpCheck = other.OpCheck;
            Summation = args.ToArray();
        }

        public bool IsOptimalMin
        {
            get
            {
                foreach (var s in DecisionBinds)
                {
                    if (s.Constant.Value < 0) { return false; }
                }
                return true;
            }
        }
        public bool IsOptimalMax
        {
            get
            {
                foreach (var s in DecisionBinds)
                {
                    if (s.Constant.Value > 0) { return false; }
                }
                return true;
            }
        }

        public decimal Value
        {
            get
            {
                decimal v = 0m;
                foreach (var a in Summation)
                {
                    v += a.Value;
                }
                return v;
            }
        }
    }
    #endregion
}
