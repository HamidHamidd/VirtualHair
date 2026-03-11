using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace VirtualHair.Models
{
    public class Friendship
    {
        public int Id { get; set; }

        [Required]
        public string RequesterId { get; set; } = string.Empty;

        [ForeignKey(nameof(RequesterId))]
        public IdentityUser? Requester { get; set; }

        [Required]
        public string AddresseeId { get; set; } = string.Empty;

        [ForeignKey(nameof(AddresseeId))]
        public IdentityUser? Addressee { get; set; }

        public bool IsAccepted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
