using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using db;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using OpenDotaDotNet;

namespace bot_2.Commands
{
    public class QueueGrabber
    {
        Context _context;
        public QueueGrabber(Context context)
        {
            _context = context;
        }
        public async Task<List<Player>> FormPlayerList(int maxPlayers)
        {
            List<Player> temp = new List<Player>();

            var list = await _context.player_queue.ToListAsync();

            List<QueueData> tempo = new List<QueueData>();
            for (int i = 0; i < maxPlayers; i++)
            {

                var record = list[i];
                ulong id = record._id;


                tempo.Add(record);



                var playerInfo = await _context.player_data.FindAsync(id);

                ulong newId = playerInfo._id;
                temp.Add(new Player(newId, playerInfo._dotammr, playerInfo._ihlmmr));
            }

            foreach (QueueData data in tempo)
            {
                _context.player_queue.Remove(data);
                await _context.SaveChangesAsync();
            }
            return temp;
        }

        public async Task<List<Player>> FormCasterList()
        {
            List<Player> temp = new List<Player>();
            var list = await _context.caster_queue.ToListAsync();

            List<CasterQueueData> tempo = new List<CasterQueueData>();
            for (int i = 0; i < list.Count; i++)
            {

                var record = list.FirstOrDefault();
                ulong id = record._id;


                tempo.Add(record);

                var c_record = await _context.player_data.FindAsync(id);
                c_record._gamestatus = 0;
                await _context.SaveChangesAsync();

                var playerInfo = await _context.player_data.FindAsync(id);

                ulong newId = playerInfo._id;
                temp.Add(new Player(newId, playerInfo._dotammr, playerInfo._ihlmmr));
            }
            foreach (CasterQueueData data in tempo)
            {
                _context.caster_queue.Remove(data);
                await _context.SaveChangesAsync();
            }
            return temp;
        }

        public async Task<List<Player>> FormSpectatorList()
        {
            List<Player> temp = new List<Player>();
            var list = await _context.spectator_queue.ToListAsync();

            List<SpectatorQueueData> tempo = new List<SpectatorQueueData>();

            for (int i = 0; i < list.Count; i++)
            {

                var record = list.FirstOrDefault();
                ulong id = record._id;

                tempo.Add(record);

                var s_record = await _context.player_data.FindAsync(id);
                s_record._gamestatus = 0;
                await _context.SaveChangesAsync();

                var playerInfo = await _context.player_data.FindAsync(id);

                ulong newId = playerInfo._id;
                temp.Add(new Player(newId, playerInfo._dotammr, playerInfo._ihlmmr));
            }
            foreach (SpectatorQueueData data in tempo)
            {
                _context.spectator_queue.Remove(data);
                await _context.SaveChangesAsync();
            }
            return temp;
        }
    }
}
