using System;

namespace FTKAPI.Utils;

/// <summary>
/// Provides support for lazy initialization.
/// </summary>
/// <typeparam name="T">Specifies the type of object that is being lazily initialized.</typeparam>
public sealed class Lazy<T> {
    private readonly object padlock = new object();
    private readonly Func<T> createValue;
    private bool isValueCreated;
    private T value;

    public T Value {
        get {
            if (!this.isValueCreated) {
                lock (this.padlock) {
                    if (!this.isValueCreated) {
                        this.value = this.createValue();
                        this.isValueCreated = true;
                    }
                }
            }
            return this.value;
        }
    }
    public bool IsValueCreated {
        get {
            lock (this.padlock) {
                return this.isValueCreated;
            }
        }
    }
    public Lazy(Func<T> createValue) {
        if (createValue == null) throw new ArgumentNullException("createValue");

        this.createValue = createValue;
    }
}