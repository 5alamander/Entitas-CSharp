using System;
using System.Collections.Generic;

namespace Entitas {

    public partial class Matcher<TEntity> {

        public static IAllOfMatcher<TEntity> AllOf(params int[] indices) {
            var matcher = new Matcher<TEntity>();
            matcher.allOfIndices = distinctIndices(indices);
            return matcher;
        }

        public static IAllOfMatcher<TEntity> AllOf(params IMatcher<TEntity>[] matchers) {
            var allOfMatcher = (Matcher<TEntity>)AllOf(mergeIndices(matchers));
            setComponentNames(allOfMatcher, matchers);
            return allOfMatcher;
        }

        public static IAnyOfMatcher<TEntity> AnyOf(params int[] indices) {
            var matcher = new Matcher<TEntity>();
            matcher.anyOfIndices = distinctIndices(indices);
            return matcher;
        }

        public static IAnyOfMatcher<TEntity> AnyOf(params IMatcher<TEntity>[] matchers) {
            var anyOfMatcher = (Matcher<TEntity>)AnyOf(mergeIndices(matchers));
            setComponentNames(anyOfMatcher, matchers);
            return anyOfMatcher;
        }

        static int[] mergeIndices(int[] allOf, int[] anyOf, int[] noneOf) {
            var indicesList = EntitasCache.GetIntList();

                if(allOf != null) {
                    indicesList.AddRange(allOf);
                }
                if(anyOf != null) {
                    indicesList.AddRange(anyOf);
                }
                if(noneOf != null) {
                    indicesList.AddRange(noneOf);
                }

                var mergedIndices = distinctIndices(indicesList);

            EntitasCache.PushIntList(indicesList);

            return mergedIndices;
        }

        static int[] mergeIndices(IMatcher<TEntity>[] matchers) {
            var indices = new int[matchers.Length];
            for (int i = 0; i < matchers.Length; i++) {
                var matcher = matchers[i];
                if(matcher.indices.Length != 1) {
                    throw new MatcherException<TEntity>(
                        "Cannot merge matchers!", matcher
                    );
                }
                indices[i] = matcher.indices[0];
            }

            return indices;
        }

        static string[] getComponentNames(IMatcher<TEntity>[] matchers) {
            for (int i = 0; i < matchers.Length; i++) {
                var matcher = matchers[i] as Matcher<TEntity>;
                if(matcher != null && matcher.componentNames != null) {
                    return matcher.componentNames;
                }
            }

            return null;
        }

        static void setComponentNames(Matcher<TEntity> matcher, IMatcher<TEntity>[] matchers) {
            var componentNames = getComponentNames(matchers);
            if(componentNames != null) {
                matcher.componentNames = componentNames;
            }
        }

        static int[] distinctIndices(IList<int> indices) {
            var indicesSet = EntitasCache.GetIntHashSet();

                foreach(var index in indices) {
                    indicesSet.Add(index);
                }
                var uniqueIndices = new int[indicesSet.Count];
                indicesSet.CopyTo(uniqueIndices);
                Array.Sort(uniqueIndices);

            EntitasCache.PushIntHashSet(indicesSet);

            return uniqueIndices;
        }
    }
}
