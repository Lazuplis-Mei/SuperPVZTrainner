using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightCheatEngine
{
    class ExpressionEval
    {
        #region 中缀转后缀
        /// <summary>
        /// 中缀表达式转换为后缀表达式
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string InfixToPostfix(string expression) {
            Stack<char> operators = new Stack<char>();
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < expression.Length; i++) {
                char ch = expression[i];
                if (char.IsWhiteSpace(ch)) continue;
                switch (ch) {
                    case '+': 
                    case '-':
                        while (operators.Count > 0) {
                            char c = operators.Pop();   //pop Operator
                            if (c == '(') {
                                operators.Push(c);      //push Operator
                                break;
                            }
                            else {
                                result.Append(c);
                            }
                        }
                        operators.Push(ch);
                        result.Append(" ");
                        break;
                    case '*': 
                    case '/':
                        while (operators.Count > 0) {
                            char c = operators.Pop();
                            if (c == '(') {
                                operators.Push(c);
                                break;
                            }
                            else {
                                if (c == '+' || c == '-') {
                                    operators.Push(c);
                                    break;
                                }
                                else {
                                    result.Append(c);
                                }
                            }
                        }
                        operators.Push(ch);
                        result.Append(" ");
                        break;
                    case '(':
                        operators.Push(ch);
                        break;
                    case ')':
                        while (operators.Count > 0) {
                            char c = operators.Pop();
                            if (c == '(') {
                                break;
                            }
                            else {
                                result.Append(c);
                            }
                        }
                        break;
                    default:
                        result.Append(ch);
                        break;
                }
            }
            while (operators.Count > 0){
                result.Append(operators.Pop()); //pop All Operator
            }
            return result.ToString();
        }
        #endregion

        #region 求值经典算法
        /// <summary>
        /// 求值的经典算法
        /// </summary>
        /// <param name="expression">字符串表达式</param>
        /// <returns></returns>
        public static int Parse(string expression)
        {
            string postfixExpression = InfixToPostfix(expression);
            Stack<int> results = new Stack<int>();
            int x, y;
            for (int i = 0; i < postfixExpression.Length; i++)
            {
                char ch = postfixExpression[i];
                if (char.IsWhiteSpace(ch)) continue;
                switch (ch)
                {
                    case '+':
                        y = results.Pop();
                        x = results.Pop();
                        results.Push(x + y);
                        break;
                    case '-':
                        y = results.Pop();
                        x = results.Pop();
                        results.Push(x - y);
                        break;
                    case '*':
                        y = results.Pop();
                        x = results.Pop();
                        results.Push(x * y);
                        break;
                    case '/':
                        y = results.Pop();
                        x = results.Pop();
                        results.Push(x / y);
                        break;
                    default:
                        int pos = i;
                        StringBuilder operand = new StringBuilder();
                        do
                        {
                            operand.Append(postfixExpression[pos]);
                            pos++;
                            if (postfixExpression.Length == pos)
                                break;
                        } while (char.IsDigit(postfixExpression[pos]) ||
                        (postfixExpression[pos] >= 'a' && postfixExpression[pos] <= 'f') ||
                        (postfixExpression[pos] >= 'A' && postfixExpression[pos] <= 'F'));
                        i = --pos;
                        results.Push(Convert.ToInt32(operand.ToString(), 16));
                        break;
                }
            }
            return results.Peek();
        }
        #endregion
    }
}
