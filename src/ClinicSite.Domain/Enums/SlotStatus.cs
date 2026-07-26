using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicSite.Domain.Enums
{
    public enum SlotStatus
    {
        Free = 1,
        Reserved = 2,
        Booked = 3,

        // The doctor closed this slot so it isn't offered to patients. Only a Free slot can be
        // blocked (never a booked or past one); a Blocked slot is never shown as available.
        Blocked = 4
    }
}
