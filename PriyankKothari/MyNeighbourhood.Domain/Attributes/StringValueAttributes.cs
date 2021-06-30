using System;

namespace MyNeighbourhood.Domain.Attributes
{
    /// <inheritdoc />
    /// <summary>
    /// Simple attribute class for storing String Values
    /// </summary>
    [Serializable]
    public class StringValueAttribute : Attribute
    {
        private readonly string _value;

        /// <inheritdoc />
        /// <summary>
        /// Creates a new <see cref="T:ToDo.Domain.Attributes.StringValueAttribute" /> instance.
        /// </summary>
        /// <param name="value">Value.</param>
        public StringValueAttribute(string value)
        {
            this._value = value;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value></value>
        public string Value => this._value;
    }
}