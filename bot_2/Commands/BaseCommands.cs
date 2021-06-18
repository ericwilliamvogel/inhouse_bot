using db;
using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.Text;

namespace bot_2.Commands
{
    public class BaseCommands : BaseCommandModule
    {
        protected static List<Action> actions = new List<Action>();
        protected Context _context;
        protected Conditions _conditions;
        protected GeneralDatabaseInfo _info;
        protected UpdatedQueue _updatedQueue;
        protected QOL QOL;
        public BaseCommands(Context context)
        {
            _context = context;
            _conditions = new Conditions(context);
            _info = new GeneralDatabaseInfo(context);
            _updatedQueue = new UpdatedQueue(context);
        }
    }
}
