using System;
using System.Collections.Generic;

namespace com.brg.Utilities
{
    public static class IntegerUtilities
    {
        private static Dictionary<int, int> _powsOfTen;

        public static int CountDigits(int n)
        {
            if (n < 100000)
            {
                // 5 or less
                if (n < 100)
                {
                    // 1 or 2
                    if (n < 10)
                        return 1;
                    else
                        return 2;
                }
                else
                {
                    // 3 or 4 or 5
                    if (n < 1000)
                        return 3;
                    else
                    {
                        // 4 or 5
                        if (n < 10000)
                            return 4;
                        else
                            return 5;
                    }
                }
            }
            else
            {
                // 6 or more
                if (n < 10000000)
                {
                    // 6 or 7
                    if (n < 1000000)
                        return 6;
                    else
                        return 7;
                }
                else
                {
                    // 8 to 10
                    if (n < 100000000)
                        return 8;
                    else
                    {
                        // 9 or 10
                        if (n < 1000000000)
                            return 9;
                        else
                            return 10;
                    }
                }
            }
        }

        public static int PowerOfTen(int n)
        {
            if (_powsOfTen == null)
            {
                _powsOfTen = new Dictionary<int, int>
                {
                    {0 , 1},
                    {1 , 10},
                    {2 , 100},
                    {3 , 1000},
                    {4 , 10000},
                    {5 , 100000},
                    {6 , 1000000},
                    {7 , 10000000},
                    {8 , 100000000},
                    {9 , 1000000000}
                };
            }

            return _powsOfTen[n];
        }

        public static bool OfSameDigits(int n)
        {
            int length = (int)Math.Log10(n) + 1;
            int m = (_powsOfTen[length] - 1) / (10 - 1);
            m *= n % 10;

            return m == n;
        }

        public static int DigitAt(int n, int pos)
        {
            var res = (n / _powsOfTen[pos]) % 10;
            return res;
        }

        public static int TopDigit(int n)
        {
            return n == 0 ? 0 : DigitAt(n, CountDigits(n) - 1);
        }
    }
}
