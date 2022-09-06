using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC2014.Core.ATAPI
{
    public enum DriveType
    {
        ACME_ROADRUNNER=1,	 // 504MB classic IDE drive
        ACME_COYOTE	=2,	     // 20MB early IDE drive */
        ACME_NEMESIS=3,	     // 20MB LBA capable drive */
        ACME_ULTRASONICUS=4, // 40MB LBA capable drive */
        ACME_ACCELLERATTI=5, // 128MB LBA capable drive */
        ACME_ZIPPIBUS=6	     // 256MB LBA capable drive */
    }
}
