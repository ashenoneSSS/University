#ifndef SORTING_ALGORITHMS_H
#define SORTING_ALGORITHMS_H

#include <vector>
using namespace std;

template<typename T>
class sort
{
public:
    virtual void sortData(vector<T>& data) = 0;
};


template<typename T>
class QckS : public sort<T>
{
private:
    void quicksort(vector<T>& data, int low, int high)
    {
        if (low < high)
        {
            int pi = partition(data, low, high);
            quicksort(data, low, pi - 1);
            quicksort(data, pi + 1, high);
        }
    }

    int partition(vector<T>& data, int low, int high)
    {
        T pivot = data[high];
        int i = (low - 1);
        for (int j = low; j < high; j++)
        {
            if (data[j] < pivot)
            {
                i++;
                swap(data[i], data[j]);
            }
        }
        swap(data[i + 1], data[high]);
        return (i + 1);
    }

public:
    void sortData(vector<T>& data) override
    {
        quicksort(data, 0, data.size() - 1);
    }
};


template<typename T>
class MrgS : public sort<T>
{
private:
    void merge(vector<T>& data, int left, int mid, int right)
    {
        int n1 = mid - left + 1;
        int n2 = right - mid;

        vector<T> Larr(n1);
        vector<T> Rarr(n2);

        for (int i = 0; i < n1; i++)
        {
            Larr[i] = data[left + i];
        }
        for (int j = 0; j < n2; j++)
        {
            Rarr[j] = data[mid + 1 + j];
        }

        int i = 0, j = 0, k = left;
        while (i < n1 && j < n2)
        {
            if (Larr[i] <= Rarr[j])
            {
                data[k] = Larr[i];
                i++;
            }
            else
            {
                data[k] = Rarr[j];
                j++;
            }
            k++;
        }

        while (i < n1)
        {
            data[k] = Larr[i];
            i++;
            k++;
        }

        while (j < n2)
        {
            data[k] = Rarr[j];
            j++;
            k++;
        }
    }

    void mergesort(vector<T>& data, int left, int right)
    {
        if (left < right)
        {
            int mid = left + (right - left) / 2;

            mergesort(data, left, mid);
            mergesort(data, mid + 1, right);
            merge(data, left, mid, right);
        }
    }

public:
    void sortData(vector<T>& data) override
    {
        mergesort(data, 0, data.size() - 1);
    }
};


template<typename T>
class InsS : public sort<T>
{
public:
    void sortData(vector<T>& data) override
    {
        int n = data.size();
        for (int i = 1; i < n; i++)
        {
            T key = data[i];
            int j = i - 1;

            while (j >= 0 && data[j] > key)
            {
                data[j + 1] = data[j];
                j--;
            }
            data[j + 1] = key;
        }
    }
};

#endif
