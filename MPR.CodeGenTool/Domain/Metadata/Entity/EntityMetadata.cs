using System.Collections.Generic;
using System.Linq;
using MPR.CodeGenTool.Domain.Metadata.Common;

namespace MPR.CodeGenTool.Domain.Metadata.Entity
{
    /// <summary>
    /// Represents an Entity class in the domain layer
    /// </summary>
    public class EntityMetadata : TypeMetadata
    {
        public List<PropertyMetadata> Properties { get; set; } = new();
        public List<PropertyMetadata> KeyProperties => Properties.Where(p => p.IsKey).ToList();
        public bool HasCompositeKey => KeyProperties.Count > 1;
        public bool IsKeyless => !KeyProperties.Any();
        public List<NavigationPropertyMetadata> NavigationProperties { get; set; } = new();
        
        public IEnumerable<PropertyMetadata> NonKeyProperties => 
            Properties.Where(p => !p.IsKey);
            
        public IEnumerable<PropertyMetadata> NonDatabaseGeneratedProperties => 
            Properties.Where(p => !p.IsDatabaseGenerated);
            
        public IEnumerable<PropertyMetadata> UpdateableProperties => 
            Properties.Where(p => !p.IsDatabaseGenerated || p.IsKey);
    }
}