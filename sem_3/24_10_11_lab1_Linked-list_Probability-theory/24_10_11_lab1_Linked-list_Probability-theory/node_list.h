#ifndef LINKED_LIST_H
#define LINKED_LIST_H


#include <iostream>
#include <vector>
using namespace std;


template<typename T>

class nodelist
{
private:
    struct Node
    {
        T data;
        Node* next;
        Node(T value) : data(value), next(nullptr) {}
    };

    Node* head;

public:
    nodelist() : head(nullptr) {}

    void add(T value)
    {
        Node* newNode = new Node(value);
        if (!head)
        {
            head = newNode;
        }
        else
        {
            Node* temp = head;
            while (temp->next)
            {
                temp = temp->next;
            }
            temp->next = newNode;
        }
    }

    vector<T> toVec()
    {
        vector<T> result;
        Node* temp = head;
        while (temp)
        {
            result.push_back(temp->data);
            temp = temp->next;
        }
        return result;
    }

    void display()
    {
        Node* temp = head;
        while (temp)
        {
            cout << temp->data << " ";
            temp = temp->next;
        }
        cout << endl;
    }
};












#endif
