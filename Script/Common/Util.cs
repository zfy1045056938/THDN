using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Reflection;
using System.Text;

public class Util 
{
   

   public static void InvokeMany(Type type,object Obj,string methodPrefix,params object[] args){
       foreach(MethodInfo method in GetMethodsByprefix(type,methodPrefix)){
           method.Invoke(Obj,args);
       }
   }

    static Dictionary<KeyValuePair<Type,string>,MethodInfo[]> lookup = new Dictionary<KeyValuePair<Type, string>, MethodInfo[]>();


    private static IEnumerable<MethodInfo> GetMethodsByprefix(Type type, string methodPrefix)
    {
        KeyValuePair<Type,string> keys = new KeyValuePair<Type, string>();
        //
        if(!lookup.ContainsKey(keys)){
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public| BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
            .Where(m=>m.Name.StartsWith(methodPrefix)).ToArray();
        lookup[keys] = methods;
        }
        return lookup[keys];
    }

    public static string PBKDF2Hash(string text, string salt)
    {
        byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
        Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(text, saltBytes, 10000);
        byte[] hash = pbkdf2.GetBytes(20);
        return BitConverter.ToString(hash).Replace("-", string.Empty);
    }

    internal static Transform GetNearestTransform(List<Transform> startPositions, Vector3 from)
    {
        throw new NotImplementedException();
    }
}
