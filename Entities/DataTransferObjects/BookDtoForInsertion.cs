using System.ComponentModel.DataAnnotations;

namespace Entities.DataTransferObjects
{
    public record BookDtoForInsertion : BookDtoForManipulation
    {
        [Required(ErrorMessage ="CategroyId is required field.")]
        public int CategoryId { get; init; }
    }
}
