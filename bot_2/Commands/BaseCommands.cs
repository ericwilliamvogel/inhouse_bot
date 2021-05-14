using db;
using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.Text;

namespace bot_2.Commands
{
    public class BaseCommands : BaseCommandModule
    {
        protected Context _context;
        protected Conditions _conditions;
        protected GeneralDatabaseUtilities _utilities;
        protected UpdatedQueue _updatedQueue;
        public BaseCommands(Context context)
        {
            _context = context;
            _conditions = new Conditions(context);
            _utilities = new GeneralDatabaseUtilities(context);
            _updatedQueue = new UpdatedQueue(context);
        }
    }
}
