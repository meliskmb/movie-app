﻿using System.ComponentModel.DataAnnotations;

namespace MovieProject.Models
{
    public class Admin
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Username { get; set; } = "";

        [Required]
        public string PasswordHash { get; set; } = "";
    }
}
