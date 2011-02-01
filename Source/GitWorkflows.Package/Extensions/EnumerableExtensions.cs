using System;
using System.Collections.Generic;
using System.Linq;

namespace GitWorkflows.Package.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Perform an action on every item of the sequence.
        /// </summary>
        /// 
        /// <typeparam name="T">The type of objects in the enumeration.</typeparam>
        /// 
        /// <param name="items">The items.</param>
        /// <param name="action">The action to perform on each item.</param>
        /// 
        /// <exception cref="ArgumentNullException"><paramref name="items"/> or 
        /// <paramref name="action"/> is <c>null</c>.</exception>
        /// 
        /// <remarks>
        ///     <para>The action is executed exactly once for each item.</para>
        /// </remarks>
        /// 
        /// <seealso cref="Do{T}"/>
        /// <seealso cref="Consume{T}"/>
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            Arguments.EnsureNotNull(new {items, action});
            foreach (var item in items)
                action(item);
        }

        /// <summary>
        /// Return a sequence that will perform the given action on every item when it is iterated.
        /// </summary>
        /// 
        /// <typeparam name="T">The type of objects in the enumeration.</typeparam>
        /// 
        /// <param name="items">The items.</param>
        /// <param name="action">The action to perform on each item.</param>
        /// 
        /// <returns>The original <paramref name="items"/> enumeration.</returns>
        /// 
        /// <exception cref="ArgumentNullException"><paramref name="items"/> or 
        /// <paramref name="action"/> is <c>null</c>.</exception>
        /// 
        /// <remarks>
        ///     <para>This method is similar to <see cref="ForEach{T}"/> method, but the important
        ///     difference is that it is lazily evaluated. The returned sequence incorporates action
        ///     execution, so that items are processed as the sequence is iterated.</para>
        /// </remarks>
        /// 
        /// <seealso cref="ForEach{T}"/>
        /// <seealso cref="Consume{T}"/>
        public static IEnumerable<T> Do<T>(this IEnumerable<T> items, Action<T> action)
        {
            Arguments.EnsureNotNull(new {items, action});
            return items.Select(
                item =>
                {
                    action(item); 
                    return item;
                }
            );
        }

        /// <summary>
        /// Consume the given sequence.
        /// </summary>
        /// 
        /// <typeparam name="T">The type of objects in the enumeration.</typeparam>
        /// 
        /// <param name="items">The items.</param>
        /// 
        /// <exception cref="ArgumentNullException"><paramref name="items"/> is <c>null</c>.
        /// </exception>
        /// 
        /// <remarks>
        ///     <para>This method is equivalent to calling <see cref="ForEach{T}"/> method with an
        ///     action that does nothing. It is intended to be used when iterating over lazy
        ///     sequences with side effects, such as those returned by <see cref="Do{T}"/> method.
        ///     </para>
        /// </remarks>
        /// 
        /// <seealso cref="ForEach{T}"/>
        /// <seealso cref="Consume{T}"/>
        public static void Consume<T>(this IEnumerable<T> items)
        {
            Arguments.EnsureNotNull(new {items});

            using (var enumerator = items.GetEnumerator())
            {
                while (enumerator.MoveNext())
                { /* Do nothing */ }
            }
        }

        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> items)
        { return items ?? Enumerable.Empty<T>(); }

        public static IEnumerable<T> DefaultIfNull<T>(this IEnumerable<T> items, T defaultValue = default(T))
        { return items ?? Enumerable.Repeat(defaultValue, 1); }

        public static IEnumerable<T> DefaultIfNullOrEmpty<T>(this IEnumerable<T> items, T defaultValue = default(T))
        { return items.EmptyIfNull().DefaultIfEmpty(defaultValue); }

        public static string ToDelimitedString(this IEnumerable<string> items, string delimiter)
        {
            Arguments.EnsureNotNull(new {items, delimiter});
            return string.Join(delimiter, items);
        }

        public static string ToDelimitedString<T>(this IEnumerable<T> items, string delimiter)
        { return items.Select(item => item.ToString()).ToDelimitedString(delimiter); }

        public static string ToCsv<T>(this IEnumerable<T> items)
        { return items.ToDelimitedString(","); }

        public static void Fill<T>(this IList<T> collection, T value)
        {
            Arguments.EnsureNotNull(new{collection});

            for (var i = 0; i < collection.Count; ++i)
                collection[i] = value;
        }
    }
}