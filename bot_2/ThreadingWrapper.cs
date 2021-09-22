using db;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace bot_2
{
    public delegate Task DbDependentAction();
    public class ThreadingWrapper
    {


        public static async Task RunDbQuery(DbDependentAction action)
        {
            using (Context context = new Context(null))
            {
                //await action(context);
            }

        }
    }
}
