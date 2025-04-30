using MPR.CodeGenTool.Domain.Metadata.Common;

namespace MPR.CodeGenTool.Domain.Metadata.Entity
{
    /// <summary>
    /// Represents navigation properties for entity relationships
    /// </summary>
    public class NavigationPropertyMetadata : PropertyMetadata
    {
        public NavigationPropertyType NavigationType { get; set; }
        public string TargetEntityName { get; set; }
        public string ForeignKeyProperty { get; set; }
    }

    /// <summary>
    /// The type of navigation property
    /// </summary>
    public enum NavigationPropertyType
    {
        OneToOne,
        OneToMany,
        ManyToOne,
        ManyToMany
    }
}