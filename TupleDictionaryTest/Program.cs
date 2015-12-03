using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace TupleTest
{
    class Program
    {
        public class Key
        {
            public int m_Key1;
            public int m_Key2;

            public Key(int k1, int k2) { m_Key1 = k1; m_Key2 = k2; }
        }

        public class KeyEqualityComparer : IEqualityComparer<Key>
        {
            #region IEqualityComparer<Key> Members

            public bool Equals(Key x, Key y)
            {
                if (x == null && y == null)
                    return true;
                else if (x == null || y == null)
                    return false;
                else
                    return x.m_Key1.Equals(y.m_Key1) && x.m_Key2.Equals(y.m_Key2);
            }

            public int GetHashCode(Key obj)
            {
                //return new { obj.m_Key1, obj.m_Key2}.GetHashCode();  //.net hash func
                // FNV hash
                unchecked // Overflow is fine, just wrap
                {
                    int hash = (int)2166136261;
                    // Suitable nullity checks etc, of course :)
                    hash = hash * 16777619 ^ obj.m_Key1;
                    hash = hash * 16777619 ^ obj.m_Key2;
                    return hash;
                }
            }

            #endregion
        }
        #region Struct
        struct StructKey : IEquatable<StructKey>
        {
            public int Int1 { get; set; }
            public int Int2 { get; set; }

            public override int GetHashCode()
            {
                unchecked // Overflow is fine, just wrap
                {
                    int hash = (int)2166136261;
                    // Suitable nullity checks etc, of course :)
                    hash = hash * 16777619 ^ Int1;
                    hash = hash * 16777619 ^ Int2;
                    return hash;
                }
            }

            public override bool Equals(object obj)
            {
                if (obj is StructKey)
                {
                    StructKey compositeKey = (StructKey)obj;

                    return ((this.Int1 == compositeKey.Int1) &&
                            (this.Int2 == compositeKey.Int2));
                }

                return false;
            }
            public bool Equals(StructKey other)
            {
                return ((this.Int1 == other.Int1) && (this.Int2 == other.Int2));
            }

            public static bool operator ==(StructKey left, StructKey right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(StructKey left, StructKey right)
            {
                return !left.Equals(right);
            }
         }
        #endregion

        static void Main(string[] args)
        {
            int itemCount = 1000000;
            Dictionary<StructKey, int> m_StructDictionary = new Dictionary<StructKey, int>();
            Dictionary<Key, int> m_KeyedDictionary = new Dictionary<Key, int>(new KeyEqualityComparer());
            Dictionary<Tuple<int, int>, int> m_TupleDictionary = new Dictionary<Tuple<int, int>, int>();
            Tuple<int, int>[] tuples = new Tuple<int, int>[itemCount];
            Key[] keys = new Key[itemCount];
            StructKey[] structKays = new StructKey[itemCount];
            int[] indices = new int[itemCount];

            // Create a million keys (and tuples) with the same random values so that we can time access into both
            // collections.
            Random rand = new Random((int)(DateTime.Now.Ticks));
            for (int n = 0; n < itemCount; ++n)
            {
                long ticks = (long)rand.Next() << 32 + rand.Next();
                while (ticks < DateTime.MinValue.Ticks || ticks > DateTime.MaxValue.Ticks)
                    ticks = (long)rand.Next() << 32 + rand.Next();

                int k1 = rand.Next();
                int k2 = rand.Next();
                keys[n] = new Key(k1, k2);
                tuples[n] = new Tuple<int, int>(k1, k2);
                structKays[n] = new StructKey{ Int1 = k1, Int2 = k2};
                indices[n] = n; // we'll shuffle later
            }

            // Shuffle the indices so that we have a (somewhat) random order to access both dictionaries.
            for (int n = 0; n < itemCount; ++n)
            {
                int i = rand.Next() % itemCount;
                int j = rand.Next() % itemCount;
                int t = indices[i];
                indices[i] = indices[j];
                indices[j] = t;
            }

            // Now we can test our access to either dictionary.
            Stopwatch timer = Stopwatch.StartNew();
            //
            timer.Reset();
            timer.Start();
            for (int n = 0; n < itemCount; ++n)
                m_TupleDictionary.Add(tuples[n], n);

            timer.Stop();
            Console.WriteLine("Tuple Dictionary insertion, {0:f03} ms ({1:f03} µs/item)", timer.Elapsed.TotalMilliseconds, timer.Elapsed.TotalMilliseconds * 1000 / itemCount);

            timer.Reset();
            timer.Start();
            int sumTupled = 0;
            for (int n = 0; n < itemCount; ++n)
                sumTupled += m_TupleDictionary[tuples[indices[n]]];

            timer.Stop();
            Console.WriteLine("Tuple Dictionary access, {0:f03} ms ({1:f03} µs/item)", timer.Elapsed.TotalMilliseconds, timer.Elapsed.TotalMilliseconds * 1000 / itemCount);
            
            timer.Reset();
            timer.Start();
            for (int n = 0; n < itemCount; ++n)
                m_KeyedDictionary.Add(keys[n], n);

            timer.Stop();
            Console.WriteLine("Keyed Dictionary insertion, {0:f03} ms ({1:f03} µs/item)", timer.Elapsed.TotalMilliseconds, timer.Elapsed.TotalMilliseconds * 1000 / itemCount);

            timer.Reset();
            timer.Start();
            int sumKeyed = 0;
            for (int n = 0; n < itemCount; ++n)
                sumKeyed += m_KeyedDictionary[keys[indices[n]]];

            timer.Stop();
            Console.WriteLine("Keyed Dictionary access, {0:f03} ms ({1:f03} µs/item)", timer.Elapsed.TotalMilliseconds, timer.Elapsed.TotalMilliseconds * 1000 / itemCount);

            #region Struct
            timer.Reset();
            timer.Start();
            for (int n = 0; n < itemCount; ++n)
                m_StructDictionary.Add(structKays[n], n);

            timer.Stop();
            Console.WriteLine("Struct Dictionary insertion, {0:f03} ms ({1:f03} µs/item)", timer.Elapsed.TotalMilliseconds, timer.Elapsed.TotalMilliseconds * 1000 / itemCount);
            timer.Reset();
            timer.Start();
            int sumStruc = 0;
            for (int n = 0; n < itemCount; ++n)
                sumStruc += m_StructDictionary[structKays[indices[n]]];

            timer.Stop();
            Console.WriteLine("Struct Dictionary access, {0:f03} ms ({1:f03} µs/item)", timer.Elapsed.TotalMilliseconds, timer.Elapsed.TotalMilliseconds * 1000 / itemCount);
            #endregion

            Console.WriteLine();
            if (sumKeyed == sumTupled)
                Console.WriteLine("Checksums match.");
            else
                Console.WriteLine("Error: Checksum mismatch!");

            Console.ReadLine();
        }
    }
}