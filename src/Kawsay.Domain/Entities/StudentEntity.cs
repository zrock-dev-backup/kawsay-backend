using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.Entities;

public class StudentEntity
{
    [Key] public int Id { get; set; }
    [Required] [MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required] public AcademicStanding Standing { get; set; } = AcademicStanding.GoodStanding;
    public ICollection<StudentModuleGrade> Grades { get; set; } = new List<StudentModuleGrade>();
}