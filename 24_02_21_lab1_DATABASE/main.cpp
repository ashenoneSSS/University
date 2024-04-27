#include <iostream>
#include <cstdio>
#include <conio.h>
#include <cstdlib>
#include <cstring>

using namespace std;

struct tree
{
    bool show_flag;
    char type[10];
    int age;
    int x, y;
} cur_tree;

void show_all_trees(FILE* fp)
{
    fseek(fp, 0, SEEK_SET);
    while (fread(&cur_tree, sizeof(tree), 1, fp))
    {
        if (cur_tree.show_flag)
        {
            printf("Type: %s, Age: %d, Position: (%d, %d)\n",
                   cur_tree.type, cur_tree.age, cur_tree.x, cur_tree.y);
        }
    }
}

void clear_file(const char* filename)
{
    FILE* fp = fopen(filename, "wb");
    if (fp != nullptr)
    {
        fclose(fp);
    }
}

int main()
{
    FILE* fp = fopen("database.dat", "r+b");
    if (!fp)
    {
        fp = fopen("database.dat", "w+b");
    }

    char choice;
    while (true)
    {
        cout << "(0) Quit\n"
             << "(1) Plant a tree\n"
             << "(2) Show all trees in Park\n"
             << "(3) Delete by type\n"
             << "(4) Delete by position\n"
             << "(5) Delete by age\n"
             << "(6) Show average age\n"
             << "(7) Clear file\n"
             << "Enter choise: ";
        choice = getch();
        system("cls");

        switch(choice)
        {
            case '0':
                fclose(fp);
                return 0;

            case '1':
            {
                cur_tree.show_flag = true;
                cout << "Enter type: ";
                cin >> cur_tree.type;
                cout << "Enter position X and Y: ";
                cin >> cur_tree.x >> cur_tree.y;
                cout << "Enter age: ";
                cin >> cur_tree.age;
                fseek(fp, 0, SEEK_END);
                fwrite(&cur_tree, sizeof(tree), 1, fp);
            } break;

            case '2':
            {
                show_all_trees(fp);
            } break;

            case '3':
            {
                cout << "Enter type to delete: ";
                char DelType[10];
                cin >> DelType;

                fseek(fp, 0, SEEK_SET);
                long int pos = 0;
                bool found = false;

                while (fread(&cur_tree, sizeof(tree), 1, fp))
                {
                    if (cur_tree.show_flag && strcmp(cur_tree.type, DelType) == 0)
                    {
                        cur_tree.show_flag = false;
                        fseek(fp, pos * sizeof(tree), SEEK_SET);
                        fwrite(&cur_tree, sizeof(tree), 1, fp);
                        fseek(fp, (pos + 1) * sizeof(tree), SEEK_SET);
                        found = true;
                    }
                    pos++;
                }

                if (!found)
                {
                    cout << "No trees of type '" << DelType << "' were found." << endl;
                }
                else
                {
                    cout << "All trees of type '" << DelType << "' were marked for deletion." << endl;
                }
            } break;


            case '4':
            {
                int DelX, DelY;
                cout << "Enter the X coordinate of the tree to delete: ";
                cin >> DelX;
                cout << "Enter the Y coordinate of the tree to delete: ";
                cin >> DelY;

                fseek(fp, 0, SEEK_SET);

                long int pos = 0;
                while (fread(&cur_tree, sizeof(tree), 1, fp))
                {
                    if (cur_tree.show_flag && cur_tree.x == DelX && cur_tree.y == DelY)
                    {
                        cur_tree.show_flag = false;
                        fseek(fp, pos * sizeof(tree), SEEK_SET);
                        fwrite(&cur_tree, sizeof(tree), 1, fp);
                        fseek(fp, (pos + 1) * sizeof(tree), SEEK_SET);
                    }
                    pos++;
                }

                cout << "Tree at position (" << DelX << ", " << DelY << ") has been deleted.\n";
            } break;

            case '5':
            {
                cout << "Enter the age of trees to delete: ";
                int DelAge;
                cin >> DelAge;

                fseek(fp, 0, SEEK_SET);
                long int pos = 0;

                while (fread(&cur_tree, sizeof(tree), 1, fp))
                {
                    if (cur_tree.show_flag && cur_tree.age == DelAge)
                    {
                        cur_tree.show_flag = false;
                        fseek(fp, pos * sizeof(tree), SEEK_SET);
                        fwrite(&cur_tree, sizeof(tree), 1, fp);
                        fseek(fp, (pos + 1) * sizeof(tree), SEEK_SET);
                    }
                    pos++;
                }

                cout << "Trees of age " << DelAge << " have been deleted.\n";
            } break;

            case '6':
            {
                cout << "Enter tree type to calculate average age: ";
                char AvarType[10];
                cin >> AvarType;

                fseek(fp, 0, SEEK_SET);
                int totalAge = 0;
                int count = 0;

                while (fread(&cur_tree, sizeof(tree), 1, fp))
                {
                    if (cur_tree.show_flag && strcmp(cur_tree.type, AvarType) == 0)
                    {
                        totalAge += cur_tree.age;
                        count++;
                    }
                }

                if (count > 0)
                {
                    double AvarAge = static_cast<double>(totalAge) / count;
                    cout << "Average age of '" << AvarType << "' trees: " << AvarAge << endl;
                }
                else
                {
                    cout << "No trees of type '" << AvarType << "' found." << endl;
                }
            } break;


            case '7':
            {
                clear_file("database.dat");
                fp = fopen("database.dat", "r+b");
            } break;

            default:
                cout << "Invalid choice. Please try again.\n";
        }
    }

    fclose(fp);
    return 0;
}

