using System;

namespace StringFormatEx
{
    [Flags]
    public enum StringFormattingOptions : byte
    {
        /// <summary>
        ///     The default behaviour.
        /// </summary>
        None = 0,
        
        /// <summary>
        ///     Indicates that holes have the dollar-sign as prefix. Overrides the recognition of holes.
        /// </summary>
        /// <remarks>
        ///     There is no way to escape the hole.
        /// <br/>
        ///     Allows for curly-brackets in the format string.
        /// <br/>
        ///     Does not unescape double curly-brackets. {{ -> { and }} -> } will not be reduced.
        /// </remarks>
        /// <example>
        ///     <c>"This is ${myName}. {He loves curly brackets!} {{But ${otherName} does not.}}"</c> is a valid format string and could be formatted as follows
        ///     <c>"This is Alex. {He loves curly brackets!} {{But Bob does not.}}"</c>
        /// </example>
        DollarLiteralHoleMode = 1 << 0,

        /// <summary>
        ///     Indicates that no exception should be thrown when the symbol of a hole is not found in the provided <see cref="FormattingArgument"/> collection. 
        /// </summary>
        /// <remarks>
        ///     Does not unescape double curly-brackets. {{ -> { and }} -> } will not be reduced.
        /// </remarks>
        DoNotThrowOnUnrecognisedSymbol = 1 << 1,
    }
}
