﻿using System;

namespace Enflow.Base
{
    public class WorkflowException : Exception
    {
        public WorkflowException(string message) : base(message) { }
    }
}
