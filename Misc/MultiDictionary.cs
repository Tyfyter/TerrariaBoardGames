using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoardGames.Misc {
	public class MultiDictionary<TKey, TValue> : IDictionary<TKey, List<TValue>>, ICollection<KeyValuePair<TKey, List<TValue>>>, IDictionary, ICollection, IReadOnlyDictionary<TKey, List<TValue>>, IReadOnlyCollection<KeyValuePair<TKey, List<TValue>>>, IEnumerable<KeyValuePair<TKey, List<TValue>>>, IEnumerable {
		private Dictionary<TKey, List<TValue>> keyValuePairs;

		public List<TValue> this[TKey key] {
			get => ((IDictionary<TKey, List<TValue>>)keyValuePairs)[key];
			set => ((IDictionary<TKey, List<TValue>>)keyValuePairs)[key] = value;
		}
		public object this[object key] {
			get => ((IDictionary)keyValuePairs)[key];
			set => ((IDictionary)keyValuePairs)[key] = value;
		}
		public MultiDictionary() {
			keyValuePairs = new Dictionary<TKey, List<TValue>>();
		}
		public MultiDictionary(Dictionary<TKey, List<TValue>> dictionary) {
			keyValuePairs = dictionary;
		}

		public ICollection<TKey> Keys => ((IDictionary<TKey, List<TValue>>)keyValuePairs).Keys;

		public ICollection<List<TValue>> Values => ((IDictionary<TKey, List<TValue>>)keyValuePairs).Values;

		public int Count => ((IDictionary<TKey, List<TValue>>)keyValuePairs).Count;

		public bool IsReadOnly => ((IDictionary<TKey, List<TValue>>)keyValuePairs).IsReadOnly;

		public bool IsFixedSize => ((IDictionary)keyValuePairs).IsFixedSize;

		public object SyncRoot => ((IDictionary)keyValuePairs).SyncRoot;

		public bool IsSynchronized => ((IDictionary)keyValuePairs).IsSynchronized;

		IEnumerable<TKey> IReadOnlyDictionary<TKey, List<TValue>>.Keys => ((IReadOnlyDictionary<TKey, List<TValue>>)keyValuePairs).Keys;

		ICollection IDictionary.Keys => ((IDictionary)keyValuePairs).Keys;

		IEnumerable<List<TValue>> IReadOnlyDictionary<TKey, List<TValue>>.Values => ((IReadOnlyDictionary<TKey, List<TValue>>)keyValuePairs).Values;

		ICollection IDictionary.Values => ((IDictionary)keyValuePairs).Values;

		public void Add(TKey key, TValue value) {
			if (ContainsKey(key)) {
				this[key].Add(value);
			} else {
				keyValuePairs.Add(key, new List<TValue> { value });
			}
		}
		[Obsolete]
		public void Add(TKey key, List<TValue> value) {
			((IDictionary<TKey, List<TValue>>)keyValuePairs).Add(key, value);
		}
		[Obsolete]
		public void Add(KeyValuePair<TKey, List<TValue>> item) {
			((IDictionary<TKey, List<TValue>>)keyValuePairs).Add(item);
		}
		[Obsolete]
		public void Add(object key, object value) {
			((IDictionary)keyValuePairs).Add(key, value);
		}
		public void Clear() {
			((IDictionary<TKey, List<TValue>>)keyValuePairs).Clear();
		}

		public bool Contains(KeyValuePair<TKey, List<TValue>> item) {
			return ((IDictionary<TKey, List<TValue>>)keyValuePairs).Contains(item);
		}

		public bool Contains(object key) {
			return ((IDictionary)keyValuePairs).Contains(key);
		}

		public bool ContainsKey(TKey key) {
			return ((IDictionary<TKey, List<TValue>>)keyValuePairs).ContainsKey(key);
		}
		public void CopyTo(KeyValuePair<TKey, List<TValue>>[] array, int arrayIndex) {
			((IDictionary<TKey, List<TValue>>)keyValuePairs).CopyTo(array, arrayIndex);
		}

		public void CopyTo(Array array, int index) {
			((IDictionary)keyValuePairs).CopyTo(array, index);
		}

		public IEnumerator<KeyValuePair<TKey, List<TValue>>> GetEnumerator() {
			return ((IDictionary<TKey, List<TValue>>)keyValuePairs).GetEnumerator();
		}
		public bool Remove(TKey key) {
			return ((IDictionary<TKey, List<TValue>>)keyValuePairs).Remove(key);
		}
		public bool Remove(KeyValuePair<TKey, List<TValue>> item) {
			return ((IDictionary<TKey, List<TValue>>)keyValuePairs).Remove(item);
		}

		public void Remove(object key) {
			((IDictionary)keyValuePairs).Remove(key);
		}

		public bool TryGetValue(TKey key, out List<TValue> value) {
			return ((IDictionary<TKey, List<TValue>>)keyValuePairs).TryGetValue(key, out value);
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return ((IDictionary<TKey, List<TValue>>)keyValuePairs).GetEnumerator();
		}

		IDictionaryEnumerator IDictionary.GetEnumerator() {
			return ((IDictionary)keyValuePairs).GetEnumerator();
		}
	}
}
