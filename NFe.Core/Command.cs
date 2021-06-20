using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.Core
{
    public abstract class Command
    {
        protected Command()
        {
            IsExecuted = false;
        }

        public bool IsExecuted { get; protected set; }
        public abstract void ExecuteAsync();
    }
}
