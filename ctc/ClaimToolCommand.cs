using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ctc
{
    public interface ClaimToolCommand
    {
        bool Run(string fullcommand);
    }
}
