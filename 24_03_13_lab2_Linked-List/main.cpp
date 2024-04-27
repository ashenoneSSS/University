#include <iostream>
using namespace std;

struct Node
{
    int coeff;
    int exp;
    Node* next;

    Node(int c, int e) : coeff(c), exp(e), next(nullptr) {}
};

class Polynomial
{
private:
    Node* head;

public:

    Polynomial() : head(nullptr) {}

    void insert(int coefficient, int exponent)
    {
        Node* newNode = new Node(coefficient, exponent);
        if (!head || head->exp < exponent)
        {
            newNode->next = head;
            head = newNode;
        }
        else
        {
            Node* temp = head;
            while (temp->next && temp->next->exp >= exponent)
                temp = temp->next;
            newNode->next = temp->next;
            temp->next = newNode;
        }
    }

    Polynomial multiply(Polynomial& that)
    {
        Polynomial result;
        for (Node* temp1 = this->head; temp1 != nullptr; temp1 = temp1->next)
        {
            for (Node* temp2 = that.head; temp2 != nullptr; temp2 = temp2->next)
            {
                int coeff = temp1->coeff * temp2->coeff;
                int exp = temp1->exp + temp2->exp;
                result.addTerm(coeff, exp);
            }
        }
        return result;
    }

    void addTerm(int coeff, int exp)
    {
        Node *temp = head, *prev = nullptr;
        while (temp && temp->exp > exp)
        {
            prev = temp;
            temp = temp->next;
        }
        if (temp && temp->exp == exp)
        {
            temp->coeff += coeff;
            if (temp->coeff == 0)
            {
                if (prev) prev->next = temp->next;
                else head = temp->next;
                delete temp;
            }
        }
        else
        {
            Node* newNode = new Node(coeff, exp);
            if (prev)
            {
                newNode->next = prev->next;
                prev->next = newNode;
            }
            else
            {
                newNode->next = head;
                head = newNode;
            }
        }
    }

    void display()
    {
        Node* temp = head;
        while (temp != nullptr)
        {
            cout << temp->coeff << "x^" << temp->exp;
            if (temp->next != nullptr) cout << " + ";
            temp = temp->next;
        }
        cout << "\n";
    }
};

int main() {
    Polynomial p1, p2;
    p1.insert(6, 2);
    p1.insert(5, 1);
    p1.insert(12, 3);
    p1.insert(7, 0);

    p2.insert(2, 1);
    p2.insert(4, 2);
    p2.insert(4, 0);

    p1.display();
    cout << " * " << endl;
    p2.display();
    cout << " = " << endl;
    Polynomial result = p1.multiply(p2);
    result.display();

    return 0;
}
