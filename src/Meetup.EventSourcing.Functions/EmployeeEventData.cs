﻿using System;

namespace Meetup.EventSourcing.Functions
{
    public class EmployeeEventData
    {
        public Guid EmployeeId { get; set; }

        public EventData Event { get; set; }
    }
}
