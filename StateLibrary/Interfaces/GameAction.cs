using System;
using System.Collections.Generic;
using System.Text;
using DomainObjects;

namespace StateLibrary.Interfaces
{
    public interface IGameAction
    {
        void Execute(Game game);
    }
}
