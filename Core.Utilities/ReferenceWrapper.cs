namespace Utilities {
    public class ReferenceWrapper<T> {
        public T Value { get; set; }
        public ReferenceWrapper(T t) {
            Value = t;
        }
        public ReferenceWrapper() {
            Value = default;
        }
    }
}
