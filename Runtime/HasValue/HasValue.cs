using System;
using System.Collections.Generic;

namespace fefek5.Toys.Runtime
{
    [Serializable]
    public struct HasValue<T> : IEquatable<HasValue<T>>, IEquatable<T>
    {
        #region Fields

        public T value;
        public bool hasValue;

        #endregion

        #region Setters
        
        public void Set(T pValue, bool pHasValue)
        {
            value = pValue;
            hasValue = pHasValue;
        }
        
        public void SetValue(T pValue) => value = pValue;
        
        public void SetHasValue(bool pHasValue) => hasValue = pHasValue;

        #endregion
        
        #region Constructors

        public HasValue(T value, bool hasValue)
        {
            this.value = value;
            this.hasValue = hasValue;
        }

        #endregion

        #region Equality

        public bool Equals(HasValue<T> other) => EqualityComparer<T>.Default.Equals(value, other.value);

        public bool Equals(T other) => EqualityComparer<T>.Default.Equals(value, other);

        public override bool Equals(object otherObject) => otherObject switch {
            HasValue<T> otherHasValue => Equals(otherHasValue),
            T otherT => Equals(otherT),
            _ => false
        };

        #endregion

        #region Implicit

        public static implicit operator T(HasValue<T> hasValue) => hasValue.value;
        
        public static implicit operator HasValue<T>(T value) => new(value, true);

        #endregion
        
        #region Operators

        public static bool operator ==(HasValue<T> left, HasValue<T> right) => left.Equals(right);
        
        public static bool operator !=(HasValue<T> left, HasValue<T> right) => !left.Equals(right);
        
        public static bool operator ==(HasValue<T> left, T right) => left.Equals(right);
        
        public static bool operator !=(HasValue<T> left, T right) => !left.Equals(right);
        
        public static bool operator ==(T left, HasValue<T> right) => right.Equals(left);
        
        public static bool operator !=(T left, HasValue<T> right) => !right.Equals(left);

        #endregion
        
        #region Others
        
        public override int GetHashCode() => value?.GetHashCode() ?? 0;

        public override string ToString() => value?.ToString() ?? string.Empty;

        #endregion
    }
}