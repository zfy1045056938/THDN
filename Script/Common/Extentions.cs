using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;



//extra classes  for method
    public static class Extentions
    {
        public static List<U> FindDup<T,U>(this List<T>t,this Func<T,U>keySelector)
        {
            return t.GroupBy(keySelector).Where(group => group.Count() > 1)
                .Select(group => group.Key).ToList();

        }

        public static int GetStableHashCode(this string code)
        {
            unchecked
            {
                int hash = 23;
                foreach (char t in code)
                {
                    hash = hash * 31 + t;
                    return hash;
                }
            }

           

        }
    }
