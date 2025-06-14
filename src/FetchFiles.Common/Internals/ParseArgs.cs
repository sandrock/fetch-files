
namespace FetchFiles.Common.Internals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    // from <https://gist.github.com/sandrock/d1fb3040e1c9326d8dd16b3bad8930ac>

    /// <summary>
    /// Helps parse CLI arguments in a loop.
    /// </summary>
    public sealed class ParseArgs : IEnumerator<string>
    {
        private readonly String[] args;
        private readonly StringComparison defaultStringComparison;
        private int index = -1;

        /// <summary>
        /// Helps parse CLI arguments in a loop.
        /// </summary>
        public ParseArgs(StringComparison defaultStringComparison, string[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            this.defaultStringComparison = defaultStringComparison;
            this.args = args.ToArray();
            this.index = -1;
        }

        /// <summary>
        /// Helps parse CLI arguments in a loop.
        /// </summary>
        public ParseArgs(string[] args)
            : this(StringComparison.OrdinalIgnoreCase, args)
        {
        }

        object IEnumerator.Current => this.Current;

        /// <summary>
        /// Gets the current argument.
        /// </summary>
        public String Current
        {
            get { return this.args[this.index]; }
        }

        /// <summary>
        /// Gets the current index.
        /// </summary>
        public int Index
        {
            get { return this.index; }
        }

        /// <summary>
        /// Checks whether the current argument is one of the specified values. 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Is(params string[] value)
        {
            return this.Is(this.defaultStringComparison, value);
        }

        /// <summary>
        /// Checks whether the current argument is one of the specified values. 
        /// </summary>
        /// <param name="stringComparison"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Is(StringComparison stringComparison, params string[] value)
        {
            foreach (var value0 in value)
            {
                if (this.Current.Equals(value0, stringComparison))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Increments the current index, moving to the next argument.
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            this.index++;
            return this.index < this.args.Length;
        }

        public void Reset()
        {
            this.index = -1;
        }

        /// <summary>
        /// Checks whether a quantity of arguments if available after the current one. 
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public bool Has(int count)
        {
            return this.args.Length - this.index - count > 0;
        }

        /// <summary>
        /// Gets the remaining items, without moving.
        /// </summary>
        /// <returns></returns>
        public string[] GetNexts()
        {
            var result = new string[this.args.Length - this.index];
            Array.Copy(this.args, this.index, result, 0, result.Length);
            return result;
        }

        /// <summary>
        /// Gets the remaining items, moving to the end.
        /// </summary>
        /// <returns></returns>
        public string[] Remains()
        {
            this.MoveNext();
            var result = new string[this.args.Length - this.index];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = this.Current;
                this.MoveNext();
            }

            return result;
        }

        public void Dispose()
        {
        }
    }
}

