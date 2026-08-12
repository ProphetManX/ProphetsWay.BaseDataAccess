using System;

namespace ProphetsWay.BaseDataAccess
{
    /// <summary>
    /// An entity that records its own lifecycle dates so it can be soft deleted rather than removed.
    /// </summary>
    /// <remarks>
    /// This interface defines the shape only. No member of this library reads or assigns these properties,
    /// and nothing here filters soft-deleted records out of a result. Stamping the dates and excluding
    /// deleted rows from reads is entirely the responsibility of the Data Access Layer implementation.
    /// </remarks>
    public interface IBaseSoftEntity : IBaseEntity
    {
        /// <summary>When the record was created.</summary>
        DateTime CreatedDate { get; set; }

        /// <summary>When the record was last modified, or <c>null</c> if it has never been modified.</summary>
        DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// When the record was soft deleted, or <c>null</c> if it is still active. Treating a non-null value
        /// as "deleted" is a convention the implementation applies; nothing in this library enforces it.
        /// </summary>
        DateTime? DeletedDate { get; set; }
    }
}
