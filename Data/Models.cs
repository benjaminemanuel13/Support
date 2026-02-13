using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Support.Data;

public class SupportArea
{
    [Key]
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }

    public ICollection<SpecificIssue> SpecificIssues { get; set; } = new List<SpecificIssue>();
}

public class SpecificIssue
{
    [Key]
    public int Id { get; set; }
    public int SupportAreaId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }

    [ForeignKey(nameof(SupportAreaId))]
    public SupportArea? SupportArea { get; set; }

    public ICollection<Solution> Solutions { get; set; } = new List<Solution>();
}

public class Solution
{
    [Key]
    public int Id { get; set; }
    public int SpecificIssueId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }

    [ForeignKey(nameof(SpecificIssueId))]
    public SpecificIssue? SpecificIssue { get; set; }
}
