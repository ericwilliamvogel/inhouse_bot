using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bot_2
{
    public class MmrCalculator
    {
        private Dictionary<string, int> _medalDictionary = new Dictionary<string, int>();

        public MmrCalculator()
        {
            SetRankDictionary();
        }
        private void SetRankDictionary()
        {
            _medalDictionary.Add("herald", 0);
            _medalDictionary.Add("guardian ", 1);
            _medalDictionary.Add("crusader", 2);
            _medalDictionary.Add("archon", 3);
            _medalDictionary.Add("legend", 4);
            _medalDictionary.Add("ancient", 5);
            _medalDictionary.Add("divine", 6);
            _medalDictionary.Add("immortal", 7);
        }
        public int GetMMR(CommandContext context, DiscordMember member)
        {
            int mmr = 0;
            foreach (KeyValuePair<string, int> pair in _medalDictionary)
            {
                var firstChar = pair.Key[0].ToString().ToUpper();
                var charar = firstChar[0];
                string newString = pair.Key.Replace(pair.Key[0], charar);
                //newString[0] = firstChar;
                //newString = pair.Key.Insert(0, firstChar.ToString().ToUpper());
                var myRole = member.Roles.FirstOrDefault(p => p.Name.ToLower().Contains(newString.ToLower()));
                if (myRole != null)
                {
                    mmr += pair.Value * 6 * 130;
                    mmr += GetMMRAdj(myRole);
                }

            }

            return mmr;
        }

        public int GetMMRAdj(DiscordRole role)
        {
            for (int i = 0; i < 6; i++)
            {
                if (role.Name.Contains(i.ToString()))
                {
                    return i * 130;
                }
            }

            return 0;
        }
    }
}
