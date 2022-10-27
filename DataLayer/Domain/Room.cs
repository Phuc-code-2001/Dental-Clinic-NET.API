﻿using DataLayer.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DataLayer.Domain
{
    public class Room : BaseEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string RoomCode { get; set; }
        public string Description { get; set; }
        public ICollection<Device> Devices { get; set; }
    }
}
