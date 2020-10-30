using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities {
    public class ObservedList<T> : IList<T>, ICollection<T>, IEnumerable<T> {
        private List<T> list;
        public List<T> Additions = new List<T>();
        public List<T> Subtractions = new List<T>();
        public ObservedList(IEnumerable<T> initial) {
            list = initial.ToList();
        }

        public T this[int index] { get { return list[index]; } set { list[index] = value; } }

        public int Count => list.Count;

        public bool IsReadOnly => false;

        public void Add(T item) {
            if (Subtractions.Contains(item)) {
                Subtractions.Remove(item);
            }
            else {
                Additions.Add(item);
            }
            list.Add(item);
        }

        public void Clear() {
            foreach(T item in list) {
                if (Additions.Contains(item)) {
                    Additions.Remove(item);
                }
                else {
                    Subtractions.Add(item);
                }
            }
            list.Clear();
        }

        public bool Contains(T item) {
            return list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator() {
            return list.GetEnumerator();
        }

        public int IndexOf(T item) {
            return list.IndexOf(item);
        }

        public void Insert(int index, T item) {
            if (Subtractions.Contains(item)) {
                Subtractions.Remove(item);
            }
            else {
                Additions.Add(item);
            }
            list.Insert(index, item);
        }

        public bool Remove(T item) {
            if (Additions.Contains(item)) {
                Additions.Remove(item);
            }
            else {
                Subtractions.Add(item);
            }
            return list.Remove(item);
        }

        public void RemoveAt(int index) {
            T item = list[index];
            if (Additions.Contains(item)) {
                Additions.Remove(item);
            }
            else {
                Subtractions.Add(item);
            }
            list.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return list.GetEnumerator();
        }
    }
}
