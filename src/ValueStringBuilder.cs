﻿using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace StringFormatEx
{
    // Source: https://source.dot.net/System.Private.CoreLib/ValueStringBuilder.cs.html
    [DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
    internal ref struct ValueStringBuilder
    {
        private char[]? _arrayToReturnToPool;
        private Span<char> _chars;
        private int _pos;

        public ValueStringBuilder(Span<char> initialBuffer)
        {
            _arrayToReturnToPool = null;
            _chars = initialBuffer;
            _pos = 0;
        }

        public ValueStringBuilder(int initialCapacity)
        {
            _arrayToReturnToPool = ArrayPool<char>.Shared.Rent(initialCapacity);
            _chars = _arrayToReturnToPool;
            _pos = 0;
        }

        public int Length
        {
            get => _pos;
            set
            {
                Debug.Assert(value >= 0);
                Debug.Assert(value <= _chars.Length);
                _pos = value;
            }
        }

        public int Capacity => _chars.Length;

        public void EnsureCapacity(int capacity)
        {
            // This is not expected to be called this with negative capacity
            Debug.Assert(capacity >= 0);

            // If the caller has a bug and calls this with negative capacity, make sure to call Grow to throw an exception.
            if ((uint)capacity > (uint)_chars.Length)
                Grow(capacity - _pos);
        }

        /// <summary>
        ///     Get a pinnable reference to the builder.
        ///     Does not ensure there is a null char after <see cref="Length" />
        ///     This overload is pattern matched in the C# 7.3+ compiler so you can omit
        ///     the explicit method call, and write eg "fixed (char* c = builder)"
        /// </summary>
        public ref char GetPinnableReference()
        {
            return ref MemoryMarshal.GetReference(_chars);
        }

        /// <summary>
        ///     Get a pinnable reference to the builder.
        /// </summary>
        /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length" /></param>
        public ref char GetPinnableReference(bool terminate)
        {
            if (terminate)
            {
                EnsureCapacity(Length + 1);
                _chars[Length] = '\0';
            }

            return ref MemoryMarshal.GetReference(_chars);
        }

        public ref char this[int index]
        {
            get
            {
                Debug.Assert(index < _pos);
                return ref _chars[index];
            }
        }

        public override string ToString()
        {
            var s = _chars.Slice(0, _pos).ToString();
            Dispose();
            return s;
        }

        private string GetDebuggerDisplay()
        {
            if (Length == 0)
                return "Length = 0, Values = []";
            StringBuilder sb = new(256);
            sb.Append("Length = ").Append(_pos);
            sb.Append(", Values = [");
            sb.Append(_chars.Slice(0, _pos).ToString());
            sb.Append(']');
            return sb.ToString();
        }

        /// <summary>Returns the underlying storage of the builder.</summary>
        public Span<char> RawChars => _chars;

        /// <summary>
        ///     Returns a span around the contents of the builder.
        /// </summary>
        /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length" /></param>
        public ReadOnlySpan<char> AsSpan(bool terminate)
        {
            if (terminate)
            {
                EnsureCapacity(Length + 1);
                _chars[Length] = '\0';
            }

            return _chars.Slice(0, _pos);
        }

        public ReadOnlySpan<char> AsSpan()
        {
            return _chars.Slice(0, _pos);
        }

        public ReadOnlySpan<char> AsSpan(int start)
        {
            return _chars.Slice(start, _pos - start);
        }

        public ReadOnlySpan<char> AsSpan(int start, int length)
        {
            return _chars.Slice(start, length);
        }

        public bool TryCopyTo(Span<char> destination, out int charsWritten)
        {
            if (_chars.Slice(0, _pos).TryCopyTo(destination))
            {
                charsWritten = _pos;
                Dispose();
                return true;
            }

            charsWritten = 0;
            Dispose();
            return false;
        }

        public void Insert(int index, char value, int count)
        {
            if (_pos > _chars.Length - count)
                Grow(count);

            int remaining = _pos - index;
            _chars.Slice(index, remaining).CopyTo(_chars.Slice(index + count));
            _chars.Slice(index, count).Fill(value);
            _pos += count;
        }

        public void Insert(int index, char value)
        {
            if (_pos > _chars.Length - 1)
                Grow(1);

            int remaining = _pos - index;
            _chars.Slice(index, remaining).CopyTo(_chars.Slice(index + 1));
            _chars[index] = value;
            _pos += 1;
        }

        public void Insert(int index, string? s)
        {
            if (String.IsNullOrEmpty(s))
                return;

            int count = s!.Length;

            if (_pos > _chars.Length - count)
                Grow(count);

            int remaining = _pos - index;
            _chars.Slice(index, remaining).CopyTo(_chars.Slice(index + count));
            s
#if !NET6_0_OR_GREATER
               .AsSpan()
#endif
               .CopyTo(_chars.Slice(index));
            _pos += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(char c)
        {
            int pos = _pos;
            if ((uint)pos < (uint)_chars.Length)
            {
                _chars[pos] = c;
                _pos = pos + 1;
            }
            else
            {
                GrowAndAppend(c);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(string? s)
        {
            if (String.IsNullOrEmpty(s))
                return;

            int pos = _pos;
            if (s!.Length == 1 && (uint)pos < (uint)_chars.Length) // very common case, e.g. appending strings from NumberFormatInfo like separators, percent symbols, etc.
            {
                _chars[pos] = s[0];
                _pos = pos + 1;
            }
            else
            {
                AppendSlow(s);
            }
        }

        private void AppendSlow(string s)
        {
            int pos = _pos;
            if (pos > _chars.Length - s.Length)
                Grow(s.Length);

            s
#if !NET6_0_OR_GREATER
               .AsSpan()
#endif
               .CopyTo(_chars.Slice(pos));
            _pos += s.Length;
        }

        public void Append(char c, int count)
        {
            if (_pos > _chars.Length - count)
                Grow(count);

            var dst = _chars.Slice(_pos, count);
            for (var i = 0; i < dst.Length; i++)
                dst[i] = c;
            _pos += count;
        }

        public unsafe void Append(char* value, int length)
        {
            if (value == (char*)0 || length == 0)
                return;
            int pos = _pos;
            if (pos > _chars.Length - length)
                Grow(length);

            var dst = _chars.Slice(_pos, length);
            for (var i = 0; i < dst.Length; i++)
                dst[i] = *value++;
            _pos += length;
        }

        public void Append(ReadOnlySpan<char> value)
        {
            int pos = _pos;
            if (pos > _chars.Length - value.Length)
                Grow(value.Length);

            value.CopyTo(_chars.Slice(_pos));
            _pos += value.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<char> AppendSpan(int length)
        {
            int origPos = _pos;
            if (origPos > _chars.Length - length)
                Grow(length);

            _pos = origPos + length;
            return _chars.Slice(origPos, length);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void GrowAndAppend(char c)
        {
            Grow(1);
            Append(c);
        }

        /// <summary>
        ///     Resize the internal buffer either by doubling current buffer size or
        ///     by adding <paramref name="additionalCapacityBeyondPos" /> to
        ///     <see cref="_pos" /> whichever is greater.
        /// </summary>
        /// <param name="additionalCapacityBeyondPos">
        ///     Number of chars requested beyond current position.
        /// </param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Grow(int additionalCapacityBeyondPos)
        {
            Debug.Assert(additionalCapacityBeyondPos > 0);
            Debug.Assert(_pos > _chars.Length - additionalCapacityBeyondPos, "Grow called incorrectly, no resize is needed.");

            // Make sure to let Rent throw an exception if the caller has a bug and the desired capacity is negative
            char[]? poolArray = ArrayPool<char>.Shared.Rent((int)Math.Max((uint)(_pos + additionalCapacityBeyondPos), (uint)_chars.Length * 2));

            _chars.Slice(0, _pos).CopyTo(poolArray);

            char[]? toReturn = _arrayToReturnToPool;
            _chars = _arrayToReturnToPool = poolArray;
            if (toReturn != null)
                ArrayPool<char>.Shared.Return(toReturn);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            char[]? toReturn = _arrayToReturnToPool;
            this = default; // for safety, to avoid using pooled array if this instance is erroneously appended to again
            if (toReturn != null)
                ArrayPool<char>.Shared.Return(toReturn);
        }
    }
}
