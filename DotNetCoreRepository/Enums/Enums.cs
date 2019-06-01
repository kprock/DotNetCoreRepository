using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreRepository.Enums
{
    public enum ProcessingNoteType
    {
        Vendor = 1,
        Customer,
        Internal
    }

    public enum ProcessingType
    {
        None = 1,
        License,
        Tracking,
        Tracking_License,
        Check_Tracking_Movement
    }

    public enum LogEventType
    {
        GENERATE_ITEMS = 1000,
        LIST_ITEMS = 1001,
        GET_ITEM,
        INSERT_ITEM,
        UPDATE_ITEM,
        DELETE_ITEM
    }

    public enum LogEventLevel
    {
        Trace = 0,
        Debug,
        Information,
        Warning,
        Error,
        Critical
    }
}
