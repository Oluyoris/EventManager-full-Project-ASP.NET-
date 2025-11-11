using System;
using System.Collections.Generic;

namespace EventManager.Api.Models
{
    public enum UserRole { Admin, Planner }
    public class User
    {
        public int Id { get; set; }
        
        public string? FullName { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CountryCode { get; set; }
        public string? Country { get; set; }
        public string? CompanyName { get; set; }
        public UserRole Role { get; set; } 
        public bool IsBlocked { get; set; }
        public bool IsEmailVerified { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<Event> Events { get; set; } = new List<Event>();
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}