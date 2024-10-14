#ifndef PROBABILITY_H
#define PROBABILITY_H

using namespace std;


class probability
{
public:
    double independ(double A, double B)
    {
        return A * B;
    }

    double expect(double prob[], double val[], int S)
    {
        double expect = 0;
        for (int i = 0; i < S; ++i)
        {
            expect += prob[i] * val[i];
        }
        return expect;
    }

    double variance(double prob[], double val[], int S, double exp)
    {
        double var = 0;
        for (int i = 0; i < S; ++i)
        {
            var += prob[i] * (val[i] - exp) * (val[i] - exp);
        }
        return var;
    }
};






#endif






