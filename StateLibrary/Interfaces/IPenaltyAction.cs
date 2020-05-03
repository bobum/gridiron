using System;
using System.Collections.Generic;
using System.Text;
using DomainObjects;

namespace StateLibrary.Interfaces
{
    interface IPenaltyAction
    {
        void Execute(Game game, PenaltyOccured occurred);
    }
}
