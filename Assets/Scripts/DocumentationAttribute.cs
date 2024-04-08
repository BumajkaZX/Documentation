namespace Documentation
{
    using System;
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    internal class DocumentationAttribute : Attribute
    {
        public string[] Names { get; private set; }
        public string Description { get; private set; }

        public DocumentationAttribute(string description, params string[] names)
        {
            Names = names;
            Description = description;
        }
    }
}
