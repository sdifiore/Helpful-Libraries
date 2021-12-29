using OrchardCore.ContentManagement;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace System.Collections.Generic
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Awaits the tasks sequentially. An alternative to <see cref="Task.WhenAll(IEnumerable{Task})"/> and
        /// <c>Nito.AsyncEx.TaskExtensions.WhenAll</c> when true multi-threaded asynchronicity is not desirable.
        /// </summary>
        /// <param name="source">A collection of items.</param>
        /// <param name="asyncOperation">An <see langword="async"/> function to call on each item.</param>
        /// <typeparam name="TItem">The type of the input collection's items.</typeparam>
        /// <typeparam name="TResult">The type of the output collection's items.</typeparam>
        /// <returns>When awaited the task contains the results which were added one-by-one.</returns>
        public static async Task<IList<TResult>> AwaitEachAsync<TItem, TResult>(
            this IEnumerable<TItem> source,
            Func<TItem, Task<TResult>> asyncOperation)
        {
            var results = new List<TResult>();
            foreach (var item in source) results.Add(await asyncOperation(item));
            return results;
        }

        /// <inheritdoc cref="AwaitEachAsync{TItem,TResult}(IEnumerable{TItem},Func{TItem,Task{TResult}})"/>
        public static async Task<IList<TResult>> AwaitEachAsync<TItem, TResult>(
            this IEnumerable<TItem> source,
            Func<TItem, int, Task<TResult>> asyncOperation)
        {
            var results = new List<TResult>();
            int index = 0;
            foreach (var item in source) results.Add(await asyncOperation(item, index++));
            return results;
        }

        /// <summary>
        /// Awaits the tasks sequentially while the action returns <see langword="false"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the <see langword="foreach"/> was never broken.</returns>
        public static async Task<bool> AwaitWhileAsync<TItem>(
            this IEnumerable<TItem> source,
            Func<TItem, Task<bool>> asyncWhileOperation)
        {
            foreach (var item in source)
            {
                if (!await asyncWhileOperation(item)) return false;
            }

            return true;
        }

        /// <summary>
        /// Awaits the tasks sequentially until the action returns <see langword="true"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the <see langword="foreach"/> was never broken.</returns>
        public static async Task<bool> AwaitUntilAsync<TItem>(
            this IEnumerable<TItem> source,
            Func<TItem, Task<bool>> asyncUntilOperation)
        {
            foreach (var item in source)
            {
                if (await asyncUntilOperation(item)) return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether any element of a sequence satisfies a condition, asynchronously, like LINQ Any().
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if any elements in the source sequence pass the test in the specified predicate;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public static async Task<bool> AnyAsync<TItem>(
            this IEnumerable<TItem> source,
            Func<TItem, Task<bool>> predicate) =>
            !await AwaitUntilAsync(source, predicate);

        /// <summary>
        /// Attempts to cast <paramref name="collection"/> into <see cref="List{T}"/>. If that's not possible then
        /// converts it into one. Not to be confused with <see cref="Enumerable.ToList{TSource}"/> that always creates a
        /// separate <see cref="List{T}"/> regardless of source type. This extension is more suitable when the <paramref
        /// name="collection"/> is expected to be <see cref="List{T}"/> but has to be stored as <see
        /// cref="IEnumerable{T}"/>.
        /// </summary>
        public static IList<T> AsList<T>(this IEnumerable<T> collection) =>
            collection is IList<T> list ? list : new List<T>(collection);

        /// <summary>
        /// Transforms the specified <paramref name="collection"/> with the <paramref name="select"/> function and
        /// returns the items that are not null. Or if the <paramref name="where"/> function is given then those that
        /// return <see langword="true"/> with it.
        /// </summary>
        public static IEnumerable<TOut> SelectWhere<TIn, TOut>(
            this IEnumerable<TIn> collection,
            Func<TIn, TOut> select,
            Func<TOut, bool> where = null)
        {
            foreach (var item in collection ?? Array.Empty<TIn>())
            {
                var converted = select(item);
                if (where?.Invoke(converted) ?? !(converted is null)) yield return converted;
            }
        }

        /// <summary>
        /// Returns a dictionary created from the <paramref name="collection"/>. If there are key clashes, the item
        /// later in the enumeration overwrites the earlier one.
        /// </summary>
        [SuppressMessage(
            "Design",
            "MA0016:Prefer return collection abstraction instead of implementation",
            Justification = "This is the point of the method.")]
        public static Dictionary<TKey, TValue> ToDictionaryOverwrite<TIn, TKey, TValue>(
            this IEnumerable<TIn> collection,
            Func<TIn, TKey> keySelector,
            Func<TIn, TValue> valueSelector)
        {
            var dictionary = new Dictionary<TKey, TValue>();
            foreach (var item in collection) dictionary[keySelector(item)] = valueSelector(item);
            return dictionary;
        }

        /// <summary>
        /// Returns a dictionary created from the <paramref name="collection"/>. If there are key clashes, the item
        /// later in the enumeration overwrites the earlier one.
        /// </summary>
        [SuppressMessage(
            "Design",
            "MA0016:Prefer return collection abstraction instead of implementation",
            Justification = "This is the point of the method.")]
        public static Dictionary<TKey, TIn> ToDictionaryOverwrite<TIn, TKey>(
            this IEnumerable<TIn> collection,
            Func<TIn, TKey> keySelector) =>
            ToDictionaryOverwrite(collection, keySelector, item => item);

        /// <summary>
        /// Returns the <paramref name="collection"/> without any duplicate items.
        /// </summary>
        /// <remarks>
        /// <para>
        /// We use <see cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource})"/>
        /// to improve compatibility. It returning <see langword="default"/> is theoretically impossible, but some DB
        /// frameworks require the "or default" after grouping.
        /// </para>
        /// </remarks>
        public static IEnumerable<TItem> Unique<TItem, TKey>(
            this IEnumerable<TItem> collection,
            Func<TItem, TKey> keySelector) =>
            collection.GroupBy(keySelector).Select(group => group.FirstOrDefault());

        /// <summary>
        /// Returns the <paramref name="collection"/> without any duplicate items picking the first of each when sorting
        /// by <paramref name="orderBySelector"/>.
        /// </summary>
        public static IEnumerable<TItem> Unique<TItem, TKey, TOrder>(
            this IEnumerable<TItem> collection,
            Func<TItem, TKey> keySelector,
            Func<TItem, TOrder> orderBySelector) =>
            collection
                .GroupBy(keySelector)
                .Select(group => group.OrderBy(orderBySelector).FirstOrDefault());

        /// <summary>
        /// Returns the <paramref name="collection"/> without any duplicate items picking the last of each when sorting
        /// by <paramref name="orderBySelector"/>.
        /// </summary>
        public static IEnumerable<TItem> UniqueDescending<TItem, TKey, TOrder>(
            this IEnumerable<TItem> collection,
            Func<TItem, TKey> keySelector,
            Func<TItem, TOrder> orderBySelector) =>
            collection
                .GroupBy(keySelector)
                .Select(group => group.OrderByDescending(orderBySelector).FirstOrDefault());

        /// <summary>
        /// Returns a string that joins the string collection. It excludes null or empty items if there are any.
        /// </summary>
        /// <returns>
        /// The concatenated texts if there are any nonempty, otherwise <see langword="null"/>.
        /// </returns>
        public static string JoinNotNullOrEmpty(this IEnumerable<string> strings, string separator = ",")
        {
            var filteredStrings = strings?.Where(text => !string.IsNullOrWhiteSpace(text)).ToList();

            return filteredStrings?.Count > 0
                ? string.Join(separator, filteredStrings)
                : null;
        }

        /// <summary>
        /// Re-flattens <see cref="ILookup{TKey, ContentItem}"/> or <c>GroupBy</c> collections and eliminates duplicates
        /// using <see cref="ContentItem.ContentItemVersionId"/>.
        /// </summary>
        public static IEnumerable<ContentItem> GetUniqueValues<TKey>(
            this IEnumerable<IGrouping<TKey, ContentItem>> lookup) =>
            lookup
                .SelectMany(grouping => grouping)
                .Unique(contentItem => contentItem.ContentItemVersionId);

        /// <summary>
        /// Re-flattens <see cref="ILookup{TKey, ContentItem}"/> or <c>GroupBy</c> collections and ensures that each
        /// grouping only had one item (i.e. one-to-one relationships).
        /// </summary>
        public static IEnumerable<ContentItem> GetSingleValues<TKey>(
            this IEnumerable<IGrouping<TKey, ContentItem>> lookup) =>
            lookup.Select(item => item.Single());

        /// <summary>
        /// A simple conditional enumeration where the items are <see langword="yield"/>ed from the <paramref
        /// name="collection"/> if the <paramref name="negativePredicate"/> returns <see langword="false"/>.
        /// </summary>
        public static IEnumerable<T> WhereNot<T>(this IEnumerable<T> collection, Func<T, bool> negativePredicate)
        {
            foreach (var item in collection)
            {
                if (!negativePredicate(item))
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Returns <paramref name="collection"/> if it's not <see langword="null"/>, otherwise <see
        /// cref="Enumerable.Empty{TResult}"/>.
        /// </summary>
        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> collection) =>
            collection ?? Enumerable.Empty<T>();

        /// <summary>
        /// Returns <paramref name="array"/> if it's not <see langword="null"/>, otherwise <see
        /// cref="Array.Empty{TResult}"/>.
        /// </summary>
        public static IEnumerable<T> EmptyIfNull<T>(this T[] array) =>
            array ?? Array.Empty<T>();

        /// <summary>
        /// Maps the provided collection of pairs using a selector with separate arguments.
        /// </summary>
        public static IEnumerable<TResult> Select<TKey, TValue, TResult>(
            this IEnumerable<KeyValuePair<TKey, TValue>> pairs,
            Func<TKey, TValue, TResult> selector)
        {
            foreach (var (key, value) in pairs)
            {
                yield return selector(key, value);
            }
        }

        /// <summary>
        /// Maps the provided collection of pairs using a selector with separate arguments.
        /// </summary>
        public static IEnumerable<TResult> Select<TKey, TValue, TResult>(
            this IEnumerable<(TKey Key, TValue Value)> pairs,
            Func<TKey, TValue, TResult> selector)
        {
            foreach (var (key, value) in pairs)
            {
                yield return selector(key, value);
            }
        }

        /// <summary>
        /// Returns the Cartesian product (also known as cross product) of <paramref name="collection1"/> with <paramref
        /// name="collection2"/>, or if that's <see langword="null"/> then with itself. This means that each possible
        /// pairing of the collections are returned. This can replace two nested for loops with a single foreach loop.
        /// </summary>
        public static IEnumerable<(T Left, T Right)> CartesianProduct<T>(
            this ICollection<T> collection1,
            ICollection<T> collection2 = null) =>
            collection1.SelectMany(_ => collection2 ?? collection1, (left, right) => (left, right));

        /// <summary>
        /// Appends every item from <paramref name="source"/> to <paramref name="target"/> using <see
        /// cref="ICollection{T}.Add(T)"/>.
        /// </summary>
        public static void AddRange<T>(this ICollection<T> target, IEnumerable<T> source)
        {
            foreach (var item in source) target.Add(item);
        }

        /// <summary>
        /// Removes every item <paramref name="collection"/> where <paramref name="predicate"/> returns <see
        /// langword="true"/> or if it's <see langword="null"/>.
        /// </summary>
        public static void RemoveAll<T>(this ICollection<T> collection, Func<T, bool> predicate = null)
        {
            foreach (var item in collection)
            {
                if (predicate == null || predicate(item))
                {
                    collection.Remove(item);
                }
            }
        }
    }
}
