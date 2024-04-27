#include <iostream>
#include <stack>
#include <string>
#include <cmath>
#include <map>
#include <sstream>
#include <cctype>

using namespace std;

class Node
{
public:
    string value;
    Node* left, * right;

    Node(string val) : value(val), left(nullptr), right(nullptr) {}
};

int precedence(char op)
{
    if (op == '#') return 4;
    if (op == '^') return 3;
    if (op == '*' || op == '/' || op == '%') return 2;
    if (op == '+' || op == '-') return 1;
    return 0;
}

bool isOperator(char c) {
    return c == '+' || c == '-' || c == '*' || c == '/' || c == '%' || c == '^' || c == '#';
}

bool isVariableCharacter(char c)
{
    return isdigit(c) || isalpha(c);
}

Node* newNode(string v)
{
    return new Node(v);
}

Node* constructExpressionTree(stack<string>& ops, stack<Node*>& nodes)
{
    string op = ops.top();
    ops.pop();

    if (op == "#")
    {
        Node* rightNode = nodes.top();
        nodes.pop();
        Node* node = newNode(op);
        node->left = new Node("0");
        node->right = rightNode;
        return node;
    }
    else
    {
        Node* rightNode = nodes.top();
        nodes.pop();
        Node* leftNode = nodes.top();
        nodes.pop();

        Node* node = newNode(op);
        node->left = leftNode;
        node->right = rightNode;
        return node;
    }
}

Node* buildExpressionTree(string expression, map<string, int>& variables)
{
    stack<Node*> nodes;
    stack<string> ops;

    for (int i = 0; i < expression.length(); i++)
    {
        if (expression[i] == ' ') continue;

        if (isVariableCharacter(expression[i]))
        {
            stringstream ss;
            while (i < expression.length() && isVariableCharacter(expression[i]))
            {
                ss << expression[i++];
            }
            i--;
            string var = ss.str();
            if (isdigit(var[0]))
            {
                nodes.push(newNode(var));
            }
            else
            {
                if (variables.find(var) == variables.end())
                {
                    cout << "Enter the value for " << var << ": ";
                    int value;
                    cin >> value;
                    variables[var] = value;
                }
                nodes.push(newNode(var));
            }
        }
        else if (expression[i] == '(')
        {
            ops.push(string(1, expression[i]));
        }
        else if (expression[i] == ')')
        {
            while (!ops.empty() && ops.top() != "(")
            {
                nodes.push(constructExpressionTree(ops, nodes));
            }
            ops.pop();
        }
        else if (isOperator(expression[i]))
        {
            if (expression[i] == '-' && (i == 0 || expression[i-1] == '(' || isOperator(expression[i-1])))
            {
                ops.push("#");
            }
            else
            {
                while (!ops.empty() && precedence(ops.top()[0]) >= precedence(expression[i]))
                {
                    nodes.push(constructExpressionTree(ops, nodes));
                }
                ops.push(string(1, expression[i]));
            }
        }
    }

    while (!ops.empty())
    {
        nodes.push(constructExpressionTree(ops, nodes));
    }

    return nodes.top();
}

int evaluate(Node* root, map<string, int>& variables)
{
    if (!root->left && !root->right)
    {
        if (isdigit(root->value[0]))
        {
            return stoi(root->value);
        }
        else
        {
            return variables[root->value];
        }
    }

    int leftVal = evaluate(root->left, variables);
    int rightVal = evaluate(root->right, variables);

    if (root->value == "+") return leftVal + rightVal;
    if (root->value == "-") return leftVal - rightVal;
    if (root->value == "*") return leftVal * rightVal;
    if (root->value == "/")
    {
        if (rightVal == 0)
        {
            cerr << "Error: Division by zero." << endl;
            return 0;
        }
        return leftVal / rightVal;
    }
    if (root->value == "%") return leftVal % rightVal;
    if (root->value == "^")
    {
        return static_cast<int>(pow(leftVal, rightVal));
    }
    if (root->value == "#") return -evaluate(root->right, variables);

    return 0;
}

void display(Node* node, string indent = "", bool last = true, bool isRoot = true)
{
    if (node != nullptr)
    {
        cout << indent;
        if (isRoot)
        {
            cout << "Root: \n";
            isRoot = false;
        }
        else
        {
            cout << (last ? "`-> " : "|-> ");
            indent += last ? "   " : "|  ";
        }

        cout << node->value << endl;
        display(node->right, indent, false, isRoot);
        display(node->left, indent, true, isRoot);
    }
}

int main()
{
    string expression ="5 + (3 -- 2) * 4 ^ 2 / (8 % 3)";
    cout << "Enter expression: ";
    //getline(cin, expression);

    map<string, int> variables;
    Node* root = buildExpressionTree(expression, variables);
    cout << "Expression Tree:" << endl;
    display(root);
    int result = evaluate(root, variables);
    cout << "Result: " << result << endl;

    return 0;
}
