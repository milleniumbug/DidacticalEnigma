﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidacticalEnigma.Utils
{
    static class EnumerableExt
    {
        public static IEnumerable<IEnumerable<T>> ChunkBy<T>(this IEnumerable<T> input, int n)
        {
            var list = new List<T>();
            int i = 0;
            foreach (var element in input)
            {
                list.Add(element);
                i++;
                if (i == n)
                {
                    yield return list;
                    list = new List<T>();
                    i = 0;
                }
            }
            if (i != 0)
                yield return list;
        }
    }
}