using System.ComponentModel;

namespace FlAdmin.Common.Models.Error;

public enum CharacterError
{
    [Description("Database encountered an unexpected error.")]
    DatabaseError,
    
}