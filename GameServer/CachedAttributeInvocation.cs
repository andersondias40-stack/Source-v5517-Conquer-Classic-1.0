using System;
using System.Collections.Generic;
using System.Reflection;

namespace COServer
{
    public class CachedAttributeInvocation<TAction, TAttribute, TKey>
        where TAttribute : Attribute
        where TAction : class
        where TKey : struct
    {

        private Dictionary<TKey, Tuple<TAttribute, TAction>> map;
        public CachedAttributeInvocation(Func<TAttribute, TKey> translator)
        {
            TKey key_d = default(TKey);
            try
            {
                var assembly = Assembly.GetCallingAssembly();
                map = new Dictionary<TKey, Tuple<TAttribute, TAction>>();
                foreach (var types in assembly.GetTypes())
                {
                    try
                    {
                        var methods = types.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        foreach (var method in methods)
                        {
                            foreach (var attribute in method.GetCustomAttributes(true))
                            {
                                try
                                {
                                    var testAttr = attribute as TAttribute;
                                    if (testAttr != null)
                                    {
                                        var key = translator(testAttr);
                                        key_d = key;
                                        var invoker = Delegate.CreateDelegate(typeof(TAction), method) as TAction;

                                        map.Add(key, new Tuple<TAttribute, TAction>(testAttr, invoker));
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(key_d);
                                    Console.WriteLine(e.ToString());
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Key:" + key_d);
                        Console.WriteException(e);
                    }
                }
            }
            catch (ReflectionTypeLoadException e) { Console.WriteException(e); }
        }
        public bool TryGetInvoker(TKey key, out Tuple<TAttribute, TAction> folded)
        {
            return map.TryGetValue(key, out folded);
        }
        public bool TryGetInvoker(TKey key, out TAction action)
        {
            Tuple<TAttribute, TAction> folded;
            if (this.TryGetInvoker(key, out folded))
            {
                action = folded.Item2;
                return true;
            }
            action = null;
            return false;
        }
    }
}
