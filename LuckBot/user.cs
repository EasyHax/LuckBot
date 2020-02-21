using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.IO;

namespace LuckBot
{
    class user
    {

        public ulong id;
        public SocketUser socket_user;

        public user(SocketUser socket_user)
        {
            (this.socket_user, id) = (socket_user, socket_user.Id);
        }

        public string file_cfg = "file_cfg.cfg";

        public List<string> cfg { get => File.ReadAllLines(file_cfg).ToList(); set => File.WriteAllLines(file_cfg, value.ToArray()); }

        public bool is_registered => cfg.Find(x => x.Contains(id.ToString())) != default;
        public bool is_admin => id == 373376762131775489;

        public void register(int[] args, string name, int ftr_attempts = 5, int graph_color = -16777216)
        {
            if (!is_registered)
            {
                var birth = parser.to_date(args);
                var line = string.Format("{0}#{1}#{2}#{3}#{4}#{5}#{6}", id, birth.Day, birth.Month, birth.Year, ftr_attempts, 0, name);
                cfg = cfg.Append(line).ToList();
            }
            else
            {
                var lines = cfg;
                var birth = parser.to_date(args);
                var line = string.Format("{0}#{1}#{2}#{3}#{4}#{5}#{6}", id, birth.Day, birth.Month, birth.Year, ftr_attempts, graph_color, name);
                lines.RemoveAt(cfg.FindIndex(x => x.Contains(id.ToString())));
                cfg = lines.Append(line).ToList();
            }
        }

        public string name => cfg.Find(x => x.Contains(id.ToString())).Split('#')[6];

        public int jocker
        {
            get => int.Parse(cfg.Find(x => x.Contains(id.ToString())).Split('#')[4]);
            set => register(new int[] { birth.Day, birth.Month, birth.Year }, name, value, graph_color);
        }

        public int graph_color
        {
            get => int.Parse(cfg.Find(x => x.Contains(id.ToString())).Split('#')[5]);
            set => register(new int[] { birth.Day, birth.Month, birth.Year }, name, jocker, value);
        }

        public DateTime birth
        {
            get
            {
                var line = cfg.Find(x => x.Contains(id.ToString()));
                var items = Array.ConvertAll(line.Split('#').Take(4).ToArray(), long.Parse);
                return parser.to_date(items.Skip(1));
            }
        }

        public int luck_at(DateTime at) => luck_meter.get_percent(birth, at);
        public int luck => luck_meter.get_percent(birth);
    }
}