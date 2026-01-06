using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.models;


namespace BackOffice.Models;

public class Schedule
{
    public int id { get; set; }
    
    [Required]
    [Range(1, 7)]
    public int Day { get; set; }
    
    [Required, MaxLength(5)]
    public string Start { get; set; } 

    [Required, MaxLength(5)]
    public string End { get; set; } 

    [Required]
    public bool Working { get; set; }

    [Required]
    public int UserId { get; set; } 

    [ForeignKey("UserId")]
    public User User { get; set; } 
}