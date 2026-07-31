using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Formula;

/// <summary>
///     <para>
///         This class represents formulas written in standard infix notation using standard precedence
///         rules. The allowed symbols are non-negative numbers written using double- precision
///         floating-point syntax; variables that consist of one or more letters followed by
///         one or more numbers; parentheses; and the four operator symbols +, -, *, and /.
///     </para>
///     <para>
///         Spaces are significant only insofar that they delimit tokens. For example,"xy" is
///         a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable;
///         and "x 23" consists of a variable "x" and a number "23". Otherwise, spaces are to be removed.
///     </para>
/// </summary>
/// <author>
///     Canyon Wirthlin and Profs Joe, Danny, Jim, and Travis
/// </author>
/// <date>
///     9/19/2025
/// </date>
public class Formula
{
    /// <summary>
    ///     All variables are letters followed by numbers. This pattern
    ///     represents valid variable name strings.
    /// </summary>
    private const string VariableRegExPattern = @"[a-zA-Z]+\d+";

    /// <summary>
    ///     String Builder for canonical representation of formula to be used during construction.
    /// </summary>
    private readonly StringBuilder _canonicalForm;

    /// <summary>
    ///     List of all tokens returned by GetTokens() during construction
    /// </summary>
    private readonly List<string> _tokens;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Formula" /> class.
    ///     <para>
    ///         Creates a Formula from a string that consists of an infix expression written as
    ///         described in the class comment. If the expression is syntactically incorrect,
    ///         throws a FormulaFormatException with an explanatory Message. See the assignment
    ///         specifications for the syntax rules you are to implement.
    ///     </para>
    ///     <para>
    ///         Non-Exhaustive Example Errors:
    ///     </para>
    ///     <list type="bullet">
    ///         <item>
    ///             Invalid variable name, e.g., x, x1x (Note: x1 is valid, but would be normalized to X1)
    ///         </item>
    ///         <item>
    ///             Empty formula, e.g., string.Empty
    ///         </item>
    ///         <item>
    ///             Mismatched Parentheses, e.g., "(("
    ///         </item>
    ///         <item>
    ///             Invalid Following Rule, e.g., "2x+5"
    ///         </item>
    ///     </list>
    /// </summary>
    /// <param name="formula"> The string representation of the formula to be created.</param>
    public Formula(string formula)
    {
        _canonicalForm = new StringBuilder();
        _tokens = GetTokens(formula);
        var parenBal = 0;
        var prevIsOperator = true;
        if (_tokens.Count == 0)
            throw new FormulaFormatException("There must be at least one token"); //   One Token Rule
        var first = _tokens[0];
        if (!(char.IsDigit(first[0]) || first == "(" || IsVar(first)))
            throw new FormulaFormatException("First token must be a number, variable, or '('"); //  First Token Rule
        for (var i = 0; i < _tokens.Count; i++)
        {
            var token = _tokens[i];
            switch (token)
            {
                case "(":
                    parenBal++;
                    prevIsOperator = true;
                    break;
                case ")":
                    if (parenBal-- < 0)
                        throw new FormulaFormatException(
                            "Parenthesis positioning/balance is incorrect"); //  Closing Parentheses Rule
                    if (prevIsOperator) throw new FormulaFormatException("Parenthesis follows an operator");
                    prevIsOperator = false;
                    break;
                case "+":
                case "-":
                case "/":
                case "*":
                    if (prevIsOperator)
                        throw new FormulaFormatException("Operator follows another operator"); //  Extra Following Rule
                    prevIsOperator = true;
                    break;
                default:
                    if (double.TryParse(token, out var num))
                    {
                        token = num.ToString(CultureInfo.InvariantCulture);
                    }
                    else if (IsVar(token))
                    {
                        token = token.ToUpper();
                        _tokens[i] = token;
                    }
                    else
                    {
                        throw new FormulaFormatException("Operand is not a valid token"); //  Valid Tokens Rule
                    }

                    if (!prevIsOperator)
                        throw new FormulaFormatException(
                            "Operand follows another operand"); //  Parenthesis/Operator Following Rule
                    prevIsOperator = false;
                    break;
            }

            _canonicalForm.Append(token);
        }

        var last = _tokens[^1];
        if (!(char.IsDigit(last[0]) || last == ")" || IsVar(last)))
            throw new FormulaFormatException("Last token must be a number, variable, or ')'"); //     Last Token Rule
        if (parenBal != 0)
            throw new FormulaFormatException("Parenthesis balance is incorrect"); //  Balanced Parentheses Rule
    }

    /// <summary>
    ///     <para>
    ///         Returns a set of all the variables in the formula.
    ///     </para>
    ///     <remarks>
    ///         Important: no variable may appear more than once in the returned set, even
    ///         if it is used more than once in the Formula.
    ///         Variables should be returned in canonical form, having all letters converted
    ///         to uppercase.
    ///     </remarks>
    ///     <list type="bullet">
    ///         <item>new("x1+y1*z1").GetVariables() should return a set containing"X1", "Y1", and "Z1".</item>
    ///         <item>new("x1+X1" ).GetVariables() should return a set containing"X1".</item>
    ///     </list>
    /// </summary>
    /// <returns> the set of variables (string names) representing the variables referenced by the formula. </returns>
    public ISet<string> GetVariables()
    {
        var variables = new HashSet<string>();
        foreach (var token in _tokens.Where(IsVar)) variables.Add(token);

        return variables;
    }

    /// <summary>
    ///     <para>
    ///         Returns a string representation of a canonical form of the formula.
    ///     </para>
    ///     <para>
    ///         The string will contain no spaces.
    ///     </para>
    ///     <para>
    ///         If the string is passed to the Formula constructor, the new Formula f
    ///         will be such that this.ToString() == f.ToString().
    ///     </para>
    ///     <para>
    ///         All the variable and number tokens in the string will be normalized.
    ///         For numbers, this means that the original string token is converted to
    ///         a number using double.Parse or double.TryParse, then converted back to a
    ///         string using double.ToString.
    ///         For variables, this means all letters are uppercased.
    ///     </para>
    ///     <para>
    ///         For example:
    ///     </para>
    ///     <code>
    /// new("x1 + Y1").ToString() should return "X1+Y1"
    /// new("x1 + 5.0000").ToString() should return "X1+5".
    /// </code>
    ///     <para>
    ///         This method should execute in O(1) time.
    ///     </para>
    /// </summary>
    /// <returns>
    ///     A canonical version (string) of the formula. All "equal" formulas
    ///     should have the same value here.
    /// </returns>
    public override string ToString()
    {
        return _canonicalForm.ToString();
    }

    /// <summary>
    ///     Reports whether "token" is a variable. It must be one or more letters
    ///     followed by one or more numbers.
    /// </summary>
    /// <param name="token"> A token that may be a variable. </param>
    /// <returns> true if the string matches the requirements, e.g., A1 or a1. </returns>
    private static bool IsVar(string token)
    {
// notice the use of ^ and $ to denote that the entire string being matched is just the variable
        var standaloneVarPattern = $"^{VariableRegExPattern}$";
        return Regex.IsMatch(token, standaloneVarPattern);
    }

    /// <summary>
    ///     <para>
    ///         Given an expression, enumerates the tokens that compose it.
    ///     </para>
    ///     <para>
    ///         Tokens returned are:
    ///     </para>
    ///     <list type="bullet">
    ///         <item>left paren</item>
    ///         <item>right paren</item>
    ///         <item>one of the four operator symbols</item>
    ///         <item>a string consisting of one or more letters followed by one or more numbers</item>
    ///         <item>a double literal</item>
    ///         <item>and anything that doesn't match one of the above patterns</item>
    ///     </list>
    ///     <para>
    ///         There are no empty tokens; white space is ignored (except to separate other tokens).
    ///     </para>
    /// </summary>
    /// <param name="formula"> A string representing an infix formula such as 1*B1/3.0. </param>
    /// <returns> The ordered list of tokens in the formula. </returns>
    private static List<string> GetTokens(string formula)
    {
        List<string> results = [];
        var lpPattern = @"\(";
        var rpPattern = @"\)";
        var opPattern = @"[\+\-*/]";
        var doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?
\d+)?";
        var spacePattern = @"\s+";
// Overall pattern
        var pattern = string.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
            lpPattern,
            rpPattern,
            opPattern,
            VariableRegExPattern,
            doublePattern,
            spacePattern);
// Enumerate matching tokens that don't consist solely of white space.
        foreach (var s in Regex.Split(formula, pattern,
                     RegexOptions.IgnorePatternWhitespace))
            if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                results.Add(s);

        return results;
    }

    /// <summary>
    ///     <para>
    ///         Reports whether f1 == f2, using the notion of equality from the <see cref="Equals" /> method.
    ///     </para>
    /// </summary>
    /// <param name="f1"> The first of two formula objects. </param>
    /// <param name="f2"> The second of two formula objects. </param>
    /// <returns> true if the two formulas are the same.</returns>
    public static bool operator ==(Formula f1, Formula f2)
    {
        return f1.Equals(f2);
    }

    /// <summary>
    ///     <para>
    ///         Reports whether f1 != f2, using the notion of equality from the <see cref="Equals" /> method.
    ///     </para>
    /// </summary>
    /// <param name="f1"> The first of two formula objects. </param>
    /// <param name="f2"> The second of two formula objects. </param>
    /// <returns> true if the two formulas are not equal to each other.</returns>
    public static bool operator !=(Formula f1, Formula f2)
    {
        return !f1.Equals(f2);
    }

    /// <summary>
    ///     <para>
    ///         Determines if two formula objects represent the same formula.
    ///     </para>
    ///     <para>
    ///         By definition, if the parameter is null or does not reference
    ///         a Formula Object then return false.
    ///     </para>
    ///     <para>
    ///         Two Formulas are considered equal if their canonical string representations
    ///         (as defined by ToString) are equal.
    ///     </para>
    /// </summary>
    /// <param name="obj"> The other object.</param>
    /// <returns>
    ///     True if the two objects represent the same formula.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return obj is Formula && ToString() == obj.ToString();
    }

    /// <summary>
    ///     <para>
    ///         Evaluates this Formula, using the lookup delegate to determine the values of
    ///         variables.
    ///     </para>
    ///     <remarks>
    ///         When the lookup method is called, it will always be passed a normalized (capitalized)
    ///         variable name. The lookup method will throw an ArgumentException if there is
    ///         not a definition for that variable token.
    ///     </remarks>
    ///     <para>
    ///         If no undefined variables or divisions by zero are encountered when evaluating
    ///         this Formula, the numeric value of the formula is returned. Otherwise, a
    ///         FormulaError is returned (with a meaningful explanation as the Reason property).
    ///     </para>
    ///     <para>
    ///         This method should never throw an exception.
    ///     </para>
    /// </summary>
    /// <param name="lookup">
    ///     <para>
    ///         Given a variable symbol as its parameter, lookup returns the variable's value
    ///         (if it has one) or throws an ArgumentException (otherwise). This method will expect
    ///         variable names to be normalized.
    ///     </para>
    /// </param>
    /// <returns> Either a double or a FormulaError, based on evaluating the formula.</returns>
    public object Evaluate(Lookup lookup)
    {
        var ops = new Stack<string>();
        var value = new Stack<double>();
        string? op;
        try
        {
            foreach (var token in _tokens)
                switch (token)
                {
                    case "+":
                    case "-":
                        if (ops.TryPeek(out op) && op is "-" or "+")
                        {
                            var right = value.Pop();
                            var left = value.Pop();
                            value.Push(ApplyOperator(ops.Pop(), left, right));
                        }

                        ops.Push(token);
                        break;
                    case "/":
                    case "*":
                    case "(":
                        ops.Push(token);
                        break;
                    case ")":
                        op = ops.Pop();
                        if (op is "+" or "-") // otherwise it's a ( and gets ignored
                        {
                            var right = value.Pop();
                            var left = value.Pop();
                            value.Push(ApplyOperator(op, left, right));
                            ops.Pop();
                        }

                        if (ops.TryPeek(out op) && op is "*" or "/")
                        {
                            var right = value.Pop();
                            var left = value.Pop();
                            value.Push(ApplyOperator(ops.Pop(), left, right));
                        }

                        break;
                    default:
                        var num = IsVar(token) ? lookup(token) : double.Parse(token);
                        if (ops.TryPeek(out op) && op is "/" or "*")
                            value.Push(ApplyOperator(ops.Pop(), value.Pop(), num));
                        else value.Push(num);
                        break;
                }

            if (ops.TryPop(out op))
            {
                var right = value.Pop();
                var left = value.Pop();
                value.Push(ApplyOperator(op, left, right));
            }
        }
        catch (DivideByZeroException)
        {
            return new FormulaError("Division by zero");
        }
        catch (ArgumentException)
        {
            return new FormulaError("Variable DNE error");
        }

        return value.Pop();
    }

    /// <summary>
    ///     Private helper method that applys a given operator to two numbers
    /// </summary>
    /// <param name="op">String Operator to apply</param>
    /// <param name="left">left Double</param>
    /// <param name="right">right Double</param>
    /// <returns>Result of operation.</returns>
    /// <exception cref="DivideByZeroException">Thrown if right is 0 and op is "/"</exception>
    private static double ApplyOperator(string op, double left, double right)
    {
        switch (op)
        {
            case "+":
                return left + right;
            case "-":
                return left - right;
            case "/":
                if (right == 0)
                    throw new DivideByZeroException("Divide by zero in formula");
                return left / right;
            case "*":
                return left * right;
            default: throw new ArgumentException("Invalid operator");
        }
    }

    /// <summary>
    ///     <para>
    ///         Returns a hash code for this Formula. If f1.Equals(f2), then it must be the
    ///         case that f1.GetHashCode() == f2.GetHashCode(). Ideally, the probability that two
    ///         randomly-generated unequal Formulas have the same hash code should be miniscule.
    ///     </para>
    /// </summary>
    /// <returns> The hashcode for the object. </returns>
    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }
}

/// <summary>
///     Used to report syntax errors in the argument to the Formula constructor.
/// </summary>
public class FormulaFormatException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FormulaFormatException" /> class.
    ///     <para>
    ///         Constructs a FormulaFormatException containing the explanatory message.
    ///     </para>
    /// </summary>
    /// <param name="message"> A developer defined message describing why the exception occured.</param>
    public FormulaFormatException(string message)
        : base(message)
    {
// All this does is call the base constructor. No extra code needed.
    }
}

/// <summary>
///     Used as a possible return value of the Formula.Evaluate method.
/// </summary>
public class FormulaError
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FormulaError" /> class.
    ///     <para>
    ///         Constructs a FormulaError containing the explanatory reason.
    ///     </para>
    /// </summary>
    /// <param name="message"> Contains a message for why the error occurred.</param>
    public FormulaError(string message)
    {
        Reason = message;
    }

    /// <summary>
    ///     Gets the reason why this FormulaError was created.
    /// </summary>
    public string Reason { get; private set; }
}

/// <summary>
///     Any method meeting this type signature can be used for
///     looking up the value of a variable.
/// </summary>
/// <exception cref="ArgumentException">
///     If a variable name is provided that is not recognized by the implementing method,
///     then the method should throw an ArgumentException.
/// </exception>
/// <param name="variableName">
///     The name of the variable (e.g., "A1") to lookup.
/// </param>
/// <returns> The value of the given variable (if one exists). </returns>
public delegate double Lookup(string variableName);