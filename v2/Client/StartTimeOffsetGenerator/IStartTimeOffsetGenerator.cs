﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Client.StartTimeOffsetGenerator
{
    public interface IStartTimeOffsetGenerator
    {
        TimeSpan Delay(TimeSpan duration);
        
    }
}
