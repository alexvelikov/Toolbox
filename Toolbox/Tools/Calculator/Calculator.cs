using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Calculator
{
    public class Calculator
    {
        private static int _errorNumber = (int)Errors.None;

        private enum Errors : byte
        {
            None,
            UnrecognizedCharacter,
            IncorectCharacterOrder,
            DividerIsNull,
            EmptyExpression
        }

        public static string Calculate(string expression)
        {
            if (expression == string.Empty) _errorNumber = (int)Errors.EmptyExpression;
            List<string> elementsList = FillElementsList(expression);

            if (_errorNumber == (int)Errors.None) CheckForStackedOperants(elementsList);
            if (_errorNumber == (int)Errors.None) ValidateExpression(elementsList);
            if (_errorNumber == (int)Errors.None) elementsList = ScanForBrackets(elementsList);
            if (_errorNumber == (int)Errors.None)
            {
                elementsList = ScanForPower(elementsList);
                elementsList = ScanForMultiplicationAndDivision(elementsList);
            }
            if (_errorNumber == (int)Errors.None)
            {
                elementsList = ScanForSubstraction(elementsList);
                elementsList = ScanForAddition(elementsList);
            }

            if (_errorNumber == (int)Errors.None) return elementsList[0];
            else return $"ERROR number {_errorNumber}";
        }

        /// <summary>
        /// Scans a string and generates a list of strings.
        /// Each element of the list is a part of a mathematical expression.
        /// Stops when the whole input is scanned or when an error in the input is detected.
        /// </summary>
        /// <param name="expression">Enter the expression that needs to be scanned.</param>
        /// <returns>A list of strings. Each element is a single part of the expression.</returns>
        /// <remarks>
        /// If a char in the input string is not recognized the scanning will stop 
        /// and leave the "errorNumber" variable with the value 4.
        /// </remarks>
        public static List<string> FillElementsList(string expression)
        {
            List<string> elementsList = new();
            string buff = "";
            int i;

            if (expression.Length >= 2 && expression[0] == '-')
            {
                buff += "-";
                i = 1;
            }
            else i = 0;

            while (i < expression.Length)
            {
                switch (expression[i])
                {
                    case ' ':
                        break;
                    case '0':
                    case '.':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        buff += expression[i];
                        break;
                    case ',':
                        buff += '.';
                        break;
                    case '+':
                    case '-':
                    case '*':
                    case '/':
                    case '^':
                    case '(':
                    case ')':
                        if (buff != "")
                        {
                            elementsList.Add(buff);
                            buff = "";
                        }
                        elementsList.Add(expression[i].ToString());
                        break;
                    default:
                        _errorNumber = (int)Errors.UnrecognizedCharacter;
                        break;
                }
                i++;
            }

            if (buff != "")
            {
                elementsList.Add(buff);
            }

            return elementsList;
        }

        public static void CheckForStackedOperants(List<string> elementsList)
        {
            for (int i = 0; i < elementsList.Count; i++)
            {
                if (i + 1 < elementsList.Count && (elementsList[i] == "+" ||
                    elementsList[i] == "-" || elementsList[i] == "*" ||
                    elementsList[i] == "/" ||
                    elementsList[i] == "^"))
                {
                    if (elementsList[i + 1] == "+" || elementsList[i + 1] == "-" ||
                        elementsList[i + 1] == "*" || elementsList[i + 1] == "/" ||
                        elementsList[i + 1] == "^" || elementsList[i + 1] == ")")
                    {
                        _errorNumber = (int)Errors.IncorectCharacterOrder;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Tries to validate the mathematical expression. 
        /// </summary>
        /// <param name="elementsList">Enter the elements list of the expression so they can be scanned by the algorithm.</param>
        /// <remarks>
        /// Checks for incorect placement of the operants.
        /// If not valid, the "errorNumber" variable's value will be changed according to the error.
        /// </remarks>
        public static void ValidateExpression(List<string> elementsList)
        {
            if (elementsList[0] == "+" || elementsList[0] == "*" || elementsList[0] == "/" || elementsList[0] == "^") // Validate that the first char is not an operant (except the -)
                _errorNumber = (int)Errors.IncorectCharacterOrder;

            int elementListCountMinusOne = elementsList.Count - 1;
            if (
                _errorNumber == (int)Errors.None &&
                (elementsList[elementListCountMinusOne] == "+" || elementsList[elementListCountMinusOne] == "-"
                || elementsList[elementListCountMinusOne] == "*" || elementsList[elementListCountMinusOne] == "/"
                || elementsList[elementListCountMinusOne] == "^")
                ) // Validate that the last char is not an operant (except the -)
                _errorNumber = (int)Errors.IncorectCharacterOrder;

            if (_errorNumber == (int)Errors.IncorectCharacterOrder)
            {
                byte additionOperantRepeatability = 0;
                byte substractionOperantRepeatability = 0;
                byte multiplicationOperantRepeatability = 0;
                byte divisionOperantRepeatability = 0;
                byte powerOperantRepeatability = 0;

                for (short i = 0; i < elementsList.Count && _errorNumber == (int)Errors.None; i++)
                {
                    switch (elementsList[i])
                    {
                        case "+":
                            if (++additionOperantRepeatability > 1)
                                _errorNumber = (int)Errors.IncorectCharacterOrder;
                            substractionOperantRepeatability = 0;
                            multiplicationOperantRepeatability = 0;
                            divisionOperantRepeatability = 0;
                            powerOperantRepeatability = 0;
                            break;
                        case "-":
                            if (++substractionOperantRepeatability > 1)
                                _errorNumber = (int)Errors.IncorectCharacterOrder;
                            additionOperantRepeatability = 0;
                            multiplicationOperantRepeatability = 0;
                            divisionOperantRepeatability = 0;
                            powerOperantRepeatability = 0;
                            break;
                        case "*":
                            if (++multiplicationOperantRepeatability > 1)
                                _errorNumber = (int)Errors.IncorectCharacterOrder;
                            additionOperantRepeatability = 0;
                            substractionOperantRepeatability = 0;
                            divisionOperantRepeatability = 0;
                            powerOperantRepeatability = 0;
                            break;
                        case "/":
                            if (++divisionOperantRepeatability > 1)
                                _errorNumber = (int)Errors.IncorectCharacterOrder;
                            additionOperantRepeatability = 0;
                            substractionOperantRepeatability = 0;
                            multiplicationOperantRepeatability = 0;
                            powerOperantRepeatability = 0;
                            break;
                        case "^":
                            if (++powerOperantRepeatability > 1)
                                _errorNumber = (int)Errors.IncorectCharacterOrder;
                            additionOperantRepeatability = 0;
                            substractionOperantRepeatability = 0;
                            multiplicationOperantRepeatability = 0;
                            divisionOperantRepeatability = 0;
                            break;
                        default:
                            additionOperantRepeatability = 0;
                            substractionOperantRepeatability = 0;
                            multiplicationOperantRepeatability = 0;
                            divisionOperantRepeatability = 0;
                            powerOperantRepeatability = 0;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Scans the entered list of strings for brackets.
        /// When a pair of brackets is detected, the algorithm calls the Calculate method recursively
        /// so the value in those brackets can be calculated and the brackets - deleted.
        /// </summary>
        /// <param name="elementsList">
        /// Enter the elements list of the expression 
        /// so the expression can be simplified by replacing the brackets in the expression 
        /// with the calculated valie within them.
        /// </param>
        /// <returns>
        /// A simplified list where the brackets in the expression
        /// are replaced with the calculated valie within them.
        /// </returns>
        /// <remarks>
        /// If the opened brackets aren't equal to the closed ones, the scanning will end
        /// and the "errorNumber" variable's value will be changed according to the error.
        /// </remarks>
        public static List<string> ScanForBrackets(List<string> elementsList)
        {
            if (elementsList.Contains("("))
            {
                if (elementsList.Contains(")"))
                {
                    short bracketsNumberAreValid = 0;
                    foreach (string element in elementsList)
                    {
                        if (element == "(")
                            bracketsNumberAreValid++;
                        else if (element == ")")
                            bracketsNumberAreValid--;
                    }

                    if (bracketsNumberAreValid == 0)
                    {
                        for (int i = 0; i < elementsList.Count; i++) // Brackets
                        {
                            string newExpression = "";
                            if (elementsList[i].Equals("("))
                            {
                                int passedBracketLayersCount = 1;

                                elementsList.RemoveAt(i);

                                while (true)
                                {
                                    if (elementsList[i].Equals("("))
                                    {
                                        passedBracketLayersCount++;
                                    }
                                    else if (elementsList[i].Equals(")"))
                                    {
                                        passedBracketLayersCount--;
                                    }

                                    if (passedBracketLayersCount == 0)
                                    {
                                        elementsList.RemoveAt(i);
                                        break;
                                    }

                                    newExpression += elementsList[i];
                                    elementsList.RemoveAt(i);
                                }
                                elementsList.Insert(i, Calculate(newExpression));
                            }
                        }
                    }
                    else _errorNumber = (int)Errors.IncorectCharacterOrder;
                }
                else _errorNumber = (int)Errors.IncorectCharacterOrder;
            }

            return elementsList;
        }

        /// <summary>
        /// Scans for the power operation in the entered list of strings.
        /// When such elements with those operation are detected,
        /// the elements on the left and right will be replaced by the value according to that operation.
        /// </summary>
        /// <param name="elementsList">
        /// Enter the elements list of the expression 
        /// so the expression can be simplified by replacing the power operations in the expression 
        /// with the calculated value between them.
        /// </param>
        /// <returns>
        /// A simplified list where the power operation signs in the expression
        /// are replaced with the calculated valie between them.
        /// </returns>
        public static List<string> ScanForPower(List<string> elementsList)
        {
            double a, b;
            short i = 0;
            while (elementsList.Contains("^"))
            {
                if (elementsList[i].Equals("^"))
                {
                    a = double.Parse(elementsList[i - 1].ToString());
                    b = double.Parse(elementsList[i + 1].ToString());
                    elementsList[i] = Math.Pow(a, b).ToString();
                    elementsList.RemoveAt(i - 1);
                    elementsList.RemoveAt(i);
                }
                else i++;
            }

            return elementsList;
        }

        /// <summary>
        /// Scans for the multiplication and division operations in the entered list of strings.
        /// When such elements with those operation are detected,
        /// the elements on the left and right will be replaced by the value according to one of those operations. 
        /// </summary>
        /// <param name="elementsList">
        /// Enter the elements list of the expression 
        /// so the expression can be simplified by replacing the multiplication or division in the expression 
        /// with the calculated between them.</param>
        /// <returns>
        /// A simplified list where the multiplication or division operations signs in the expression
        /// are replaced with the calculated valie between them.
        /// </returns>
        /// <remarks>
        /// If the algorithm is forced to divide by 0, the scanning will stop
        /// and the "errorNumber" variable's value will be changed according to the error.
        /// </remarks>
        public static List<string> ScanForMultiplicationAndDivision(List<string> elementsList)
        {
            double a, b;
            short i = 0;
            while (elementsList.Contains("*") || elementsList.Contains("/"))
            {
                if (elementsList[i].Equals("*"))
                {
                    a = double.Parse(elementsList[i - 1].ToString());
                    b = double.Parse(elementsList[i + 1].ToString());
                    elementsList[i] = (a * b).ToString();
                    elementsList.RemoveAt(i - 1);
                    elementsList.RemoveAt(i);
                }
                else if (elementsList[i].Equals("/"))
                {
                    if (elementsList[i + 1].ToString() == "0")
                    {
                        _errorNumber = (int)Errors.DividerIsNull;
                        break;
                    }
                    a = double.Parse(elementsList[i - 1].ToString());
                    b = double.Parse(elementsList[i + 1].ToString());
                    elementsList[i] = (a / b).ToString();
                    elementsList.RemoveAt(i - 1);
                    elementsList.RemoveAt(i);
                }
                else i++;
            }

            return elementsList;
        }

        /// <summary>
        /// Scans for the substraction operation in the entered list of strings.
        /// When such elements with this operation is detected,
        /// the elements on the left and right will be replaced by the value according to that operation. 
        /// </summary>
        /// <param name="elementsList">
        /// Enter the elements list of the expression 
        /// so the expression can be simplified by replacing the substraction operations in the expression 
        /// with the calculated value between them.</param>
        /// <returns>
        /// A simplified list where the multiplication or division operation signs in the expression
        /// are replaced with the calculated value between them.
        /// </returns>
        public static List<string> ScanForSubstraction(List<string> elementsList)
        {
            double a, b;
            short i = 0;
            while (elementsList.Contains("-"))
            {
                if (elementsList[i].Equals("-"))
                {
                    if (i == 0)
                    {
                        elementsList.RemoveAt(i);
                        elementsList[i] = (int.Parse(elementsList[i]) * (-1)).ToString();
                    }
                    else
                    {
                        a = double.Parse(elementsList[i - 1].ToString());
                        b = double.Parse(elementsList[i + 1].ToString());
                        elementsList[i] = (a - b).ToString();
                        elementsList.RemoveAt(i - 1);
                        elementsList.RemoveAt(i);
                    }
                }
                else i++;
            }

            return elementsList;
        }

        /// <summary>
        /// Scans for the addition operation in the entered list of strings.
        /// When such elements with this operation is detected,
        /// the elements on the left and right will be replaced by the value according to that operation. 
        /// </summary>
        /// <param name="elementsList">
        /// Enter the elements list of the expression 
        /// so the expression can be simplified by replacing the addition operations in the expression 
        /// with the calculated value between them.</param>
        /// <returns>
        /// A simplified list where the addition operation signs in the expression
        /// are replaced with the calculated value between them.
        /// </returns>
        public static List<string> ScanForAddition(List<string> elementsList)
        {
            double a, b;
            short i = 0;
            while (elementsList.Contains("+"))
            {
                if (elementsList[i].Equals("+"))
                {
                    a = double.Parse(elementsList[i - 1].ToString());
                    b = double.Parse(elementsList[i + 1].ToString());
                    elementsList[i] = (a + b).ToString();
                    elementsList.RemoveAt(i - 1);
                    elementsList.RemoveAt(i);
                }
                else i++;
            }

            return elementsList;
        }
    }
}
