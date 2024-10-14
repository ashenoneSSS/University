#include "sorting.h"
#include "node_list.h"
#include "probability.h"
#include <iostream>
#include <vector>

using namespace std;

void display()
{
    cout << "0. Exit\n";
    cout << "1. Enter values  in linked list\n";
    cout << "2. Enter sorting method\n";
    cout << "3. Independent events\n";
    cout << "4. Mathematical expectation and variance\n\n";
    cout << "Enter one of options: ";
}

int main()
{
    nodelist<int> list;
    vector<int> vec;
    probability prob;
    int c;

    do
    {
        display();
        cin >> c;

        switch (c)
        {
        case 1:
        {
            int n_1, v_1;
            cout << "Ammount of nodes: ";
            cin >> n_1;

            if (n_1 <= 0)
            {
                cout << "Number of nodes must be greater than zero\n";
                break;
            }

            for (int i = 0; i < n_1; i++)
            {
                cout << "Enter value " << (i + 1) << ": ";
                cin >> v_1;
                list.add(v_1);
            }

            cout << "linked list: ";
            list.display();
            vec = list.toVec();
            break;
        }

        case 2:
        {
            if (vec.empty())
            {
                cout << "linked list is empty. Enter some values to it(Option 1)\n";
                break;
            }

            int sortChoice;
            cout << endl << "Enter sorting algorithm:\n";
            cout << "1. Quick sort\n";
            cout << "2. Merge sort\n";
            cout << "3. Insertion sort\n\n";
            cin >> sortChoice;

            switch (sortChoice)
            {
            case 1:
            {
                QckS<int> quick;
                quick.sortData(vec);
                cout << "Sorted list: ";
                break;
            }
            case 2:
            {
                MrgS<int> merge;
                merge.sortData(vec);
                cout << "Sorted list: ";
                break;
            }
            case 3:
            {
                InsS<int> insert;
                insert.sortData(vec);
                cout << "Sorted list: ";
                break;
            }
            default:
                cout << "Invalid sorting choice.\n";
                break;
            }


            for (const auto& v_2 : vec)
            {
                cout << v_2 << " ";
            }

            cout << endl;
            break;
        }

        case 3:
        {
            int n_3;
            cout << "Ammount of events: ";
            cin >> n_3;

            if (n_3 <= 0)
            {
                cout << "Number of events must be greater than zero\n";
                break;
            }

            vector<double> prob_3(n_3);
            double S = 1.0;

            for (int i = 0; i < n_3; i++)
            {
                cout << "Enter probability of event: " << (i + 1);
                cin >> prob_3[i];

                if (prob_3[i] < 0 || prob_3[i] > 1)
                {
                    cout << "Probability must be between 0 and 1.\n";
                    S = -1;
                    break;
                }

                S *= prob_3[i];
            }

            if (S >= 0)
            {
                cout << "Probability of all events: " << S << endl;
            }

            break;
        }


        case 4:
        {
            int n_4;
            cout << "Enter number of values: ";
            cin >> n_4;

            if (n_4 <= 0)
            {
                cout << "The number of values must be greater than zero\n";
                break;
            }

            vector<double> prob_4(n_4);
            vector<double> v_4(n_4);

            for (int i = 0; i < n_4; i++)
            {
                cout << "Enter probability of event " << (i + 1) << ": ";
                cin >> prob_4[i];

                if (prob_4[i] < 0 || prob_4[i] > 1)
                {
                    cout << "Probability must be between 0 and 1\n";
                    break;
                }

                cout << "Enter value for event " << (i + 1) << ": ";
                cin >> v_4[i];
            }

            double exp = prob.expect(prob_4.data(), v_4.data(), n_4);
            cout << "Mathematical expectation: " << exp << endl;

            double var = prob.variance(prob_4.data(), v_4.data(), n_4, exp);
            cout << "Variance: " << var << endl;

            break;
        }

        case 0:
            cout << "Exiting program\n";
            break;

        default:
            cout << "Invalid choice\n";
        }
    } while (c != 0);

    return 0;
}
