using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace LuckBot
{
    class parser
    {

        public static T[] parse<T>(string message, int args_count = -1, int start = 0)
        {
            var args = new List<T>();

            try
            {
                var args1 = message.Split(' ').Skip(1);
                var args2 = args_count != -1 ? args1.Take(args_count) : args1;

                foreach (var arg in args2)
                    args.Add((T)Convert.ChangeType(arg, typeof(T)));

                return args.ToArray();
            }
            catch
            {
                return args.ToArray();
            }
        }

        public static (DateTime, DateTime) parse_date(int[] args)
        {
            var from_date = new DateTime();
            var to_date = new DateTime();

            if (args.Length == 1)
            {
                from_date = DateTime.Now.AddDays(-args[0]);
                to_date = DateTime.Now;
            }
            else if (args.Length == 2)
            {
                from_date = DateTime.Now.AddDays(-args[0]);
                to_date = from_date.AddDays(args[1]);
            }
            else
            {
                from_date = parser.to_date(args);
                to_date = parser.to_date(args.Skip(3));
            }

            return (from_date, to_date);
        }

        public static DateTime to_date(IEnumerable<int> items) => new DateTime(items.ElementAt(2), items.ElementAt(1), items.ElementAt(0));
        public static DateTime to_date(IEnumerable<long> items) => to_date(items.Select(x => int.Parse(x.ToString())));

        public static string format_message(string message) => fix($"```css\n{message}\n```");
        public static string fix(string message) => message.Replace("Eжей", "Exen");

        public static string date_to_string(DateTime at_date) => $"{at_date.Day}/{at_date.Month}/{at_date.Year}";

        public static user[] get_mentions(SocketCommandContext context)
        {
            var users = get_users(context);

            if (context.Message.ToString().Contains("-all"))
                return users;

            var m_users = new List<user>();
            foreach (var user in users)
                if (context.Message.ToString().ToLower().Contains(user.name.ToLower()))
                    m_users.Add(user);
            return m_users.ToArray();
        }

        public static ulong[] get_ids()
        {
            var ids = new List<ulong>();
            foreach(var line in System.IO.File.ReadAllLines("file_cfg.cfg"))
                ids.Add(ulong.Parse(line.Split('#')[0]));
            return ids.ToArray();
        }

        public static user[] get_users(SocketCommandContext context) => get_ids().Select(x => new user(context.Guild.GetUser(x))).ToArray();

        public static async Task<(DateTime, DateTime, user[])> get_graph_args(SocketCommandContext context)
        {
            var user = new user(context.Message.Author);
            var users = get_mentions(context);
            var args = parse<int>(context.Message.ToString());

            var (from_date, to_date) = parse_date(args);

            if (args.Length == 1) await context.Channel.SendMessageAsync(format_message($"Graphique des {args[0]} derniers jours"));
            if (args.Length == 2) await context.Channel.SendMessageAsync(format_message($"Graphique d'il y a {args[0]} jours pendant {args[1]} jours"));

            if (from_date.Ticks > DateTime.Now.Ticks || to_date.Ticks > DateTime.Now.Ticks)
            {
                user.jocker--;
                var jocker = user.jocker > 1 ? "jockers" : "jocker";
                await context.Channel.SendMessageAsync("Bien joué mécréant, tu viens de perdre un joker en essayant de m'enculer.");
                await context.Channel.SendMessageAsync(format_message($"{user.name} a maintenant {user.jocker} {jocker}"));
                return (DateTime.Now, DateTime.Now, null);
            }

            return (from_date, to_date, users.Length == 0 ? new user[] { user } : users);
        }
    }
}
