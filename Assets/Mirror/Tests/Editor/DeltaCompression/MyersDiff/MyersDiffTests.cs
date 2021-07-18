// original paper: http://www.xmailserver.org/diff2.pdf
// used in diff, git!
using System;
using System.Collections.Generic;
using MyersDiff;
using NUnit.Framework;
using UnityEngine;

namespace Mirror.Tests.DeltaCompression
{
    public class MyersDiffTests : DeltaCompressionTests
    {
        // like Diff.Item, but with the actual deleted/inserted values
        public struct Modified
        {
            // Start Line number in Data A.
            public int StartA;
            // Start Line number in Data B.
            public int StartB;

            // Number of deletions in Data A.
            public int deletedA;
            // actual values inserted into Data B.
            public List<int> insertedB;

            // serialize into a writer so we can send it over the network
            public void Serialize(NetworkWriter writer)
            {

            }

            public void Deserialize(NetworkReader reader)
            {
                // TODO
            }

            // tostring for easier debugging / understanding
            public override string ToString()
            {
                string s = "";

                // deleted
                if (deletedA > 0)
                    s += $"StartA={StartA} StartB={StartB}: deleted {deletedA} entries; ";

                // inserted
                foreach (int value in insertedB)
                    s += $"StartA={StartA} StartB={StartB}: inserted value='{value}'; ";

                return s;
            }
        }

        // helper function to convert Diff.Item[] to an actual patch
        public static List<Modified> MakePatch(int[] A, int[] B, Diff.Item[] diffs)
        {
            // an item has:
            //   StartA
            //   StartB
            //   DeletedA
            //   InsertedB
            //
            // to make an actual patch, we need to also included the insered values.
            // (the deleted values are deleted. we don't need to include those.)
            List<Modified> result = new List<Modified>();
            foreach (Diff.Item item in diffs)
            {
                Modified modified = new Modified();
                modified.StartA = item.StartA;
                modified.StartB = item.StartB;
                modified.deletedA = item.deletedA;
                // add the inserted values
                modified.insertedB = new List<int>();
                for (int i = 0; i < item.insertedB; ++i)
                {
                    modified.insertedB.Add(A[item.StartA]);
                }
                result.Add(modified);
            }
            return result;
        }

        public override void ComputeDelta(NetworkWriter from, NetworkWriter to, NetworkWriter result)
        {
            // algorithm needs int[] for now
            byte[] fromBytes = from.ToArray();
            int[] fromInts = new int[fromBytes.Length];
            for (int i = 0; i < fromBytes.Length; ++i)
                fromInts[i] = fromBytes[i];

            byte[] toBytes = to.ToArray();
            int[] toInts = new int[toBytes.Length];
            for (int i = 0; i < toBytes.Length; ++i)
                toInts[i] = toBytes[i];

            Diff.Item[] items = Diff.DiffInt(fromInts, toInts);
        }

        public override void ApplyPatch(NetworkWriter from, NetworkWriter patch, NetworkWriter result)
        {
            throw new NotImplementedException();
        }

        // simple test for understanding
        [Test]
        public void SimpleTest()
        {
            // test values larger than indices for easier reading
            int[] A = {11, 22,         33, 44, 44};
            int[] B = {11, 22, 22, 22, 33,     44};
            Debug.Log($"A={String.Join(", ", A)}");
            Debug.Log($"B={String.Join(", ", B)}");

            // myers diff
            Diff.Item[] items = Diff.DiffInt(A, B);
            foreach (Diff.Item item in items)
                Debug.Log($"item: startA={item.StartA} startB={item.StartB} deleteA={item.deletedA} insertB={item.insertedB}");

            // make patch
            List<Modified> patch = MakePatch(A, B, items);
            foreach (Modified modified in patch)
                Debug.Log("modified: " + modified);
        }
    }
}