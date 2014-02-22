﻿using System;
using System.Collections.Generic;

namespace CallMeMaybe
{
    public struct Maybe<T> : IEquatable<Maybe<T>>, IMaybe<T>
    {
        private readonly T _value;
        private readonly bool _hasValue;

        public bool HasValue
        {
            get { return _hasValue; }
        }

        public Maybe(T value)
        {
            _hasValue = true;
            _value = value;
        }

        bool IMaybe.TryGetValue(out object value)
        {
            value = _value;
            return _hasValue;
        }

        #region Equality

        public bool Equals(Maybe<T> other)
        {
            return _hasValue == other._hasValue && 
                (!_hasValue || EqualityComparer<T>.Default.Equals(_value, other._value));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            var maybe = obj as IMaybe;
            if (maybe == null) return false;
            object value;
            if (!maybe.TryGetValue(out value))
            {
                // If the other one doesn't have a value, then we're
                // only "equal" if this one doesn't either.
                return !HasValue;
            }
            return Equals(_value, value);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_hasValue.GetHashCode()*397) ^ EqualityComparer<T>.Default.GetHashCode(_value);
            }
        }

        public static bool operator ==(Maybe<T> left, Maybe<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Maybe<T> left, Maybe<T> right)
        {
            return !left.Equals(right);
        }

        #endregion
    }

    public interface IMaybe
    {
        bool HasValue { get; }
        bool TryGetValue(out object value);
    }

    public interface IMaybe<out T> : IMaybe
    {
    }

    public static class Maybe
    {
        public static Maybe<T> From<T>(T value)
        {
            return new Maybe<T>(value);
        }

        public static Maybe<T> Empty<T>()
        {
            return new Maybe<T>();
        }
    }
}