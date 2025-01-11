using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class WeeklyTaskCreate
{
    [Required]
    public string MonthId { get; set; }

    [Required]
    public int WeekNumber { get; set; }

    [Required]
    public string StartDate { get; set; }

    [Required]
    public string EndDate { get; set; }

    public List<int> Days { get; set; } = new List<int>();

    [Required]
    public string Color { get; set; }

    [Required]
    public string TaskText { get; set; }
} 