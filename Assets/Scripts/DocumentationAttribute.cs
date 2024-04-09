namespace Documentation
{
    using System;
    
    /// <summary>
    /// Атрибут для документации
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = false)]
    internal class DocumentationAttribute : Attribute
    {
        public string[] Names { get; private set; }
        public string Description { get; private set; }
        
        /// <param name="description">Описание скрипта</param>
        /// <param name="names">Теги\имена</param>
        public DocumentationAttribute(string description, params string[] names)
        {
            Names = names;
            Description = description;
        }
    }
}
