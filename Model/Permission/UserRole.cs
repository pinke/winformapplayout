﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Permission
{
    public class UserRole
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public RoleType RoleId { get; set; }
    }
}
