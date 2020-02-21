using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using Discord.Commands;
using ZedGraph;

namespace LuckBot
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command( "register", true )]
        public async Task register()
        {
            var user = new user(Context.Message.Author);
            var args = parser.parse<int>(Context.Message.ToString());
            var username = Context.Guild.GetUser(Context.User.Id).Nickname;
            var name = username.Contains(' ') ? username.Remove(username.IndexOf(' ')) : username;

            if ( user.is_registered )
            {
                user.register( args, name, user.jocker, user.graph_color );
                await ReplyAsync( parser.format_message( "Tes informations ont ete mises a jour !" ) );
                return;
            }

            user.register( args, name );
            await ReplyAsync( parser.format_message( $"Bienvenue dans la secte Avorton !" ) );
        }

        [Command( "moy", true )]
        public async Task moy()
        {
            var users = parser.get_mentions(Context);
            var args = parser.parse<int>(Context.Message.ToString());
            var (from_date, to_date) = parser.parse_date( args );
            var (s_from_date, s_to_date) = (parser.date_to_string( from_date ), parser.date_to_string( to_date ));

            var msg = default(string);
            var moys = new Dictionary<string, double>();

            foreach ( var user in users )
            {
                var lucks = new List<int>();

                for ( var d = from_date; d <= to_date; d = d.AddDays( 1 ) )
                    lucks.Add( user.luck_at( d ) );

                var moy = Math.Round(Enumerable.Average(lucks), 2);
                moys.Add( user.name, moy );
            }

            var o_moys = from entry in moys orderby entry.Value descending select entry;

            foreach ( var moy in o_moys )
            {
                var s_moy = moy.Value.ToString().Replace(".", ",");
                msg += $"{moy.Key}{new string( ' ', 8 - moy.Key.Length )} => moy[{s_from_date};{s_to_date}] = {s_moy}\n";
            }

            await ReplyAsync( parser.format_message( msg ) );
        }

        [Command( "all", true )]
        public async Task all()
        {
            var msg = default(string);
            var dic = new Dictionary<string, int>();

            foreach ( var user in parser.get_users( Context ) )
                dic.Add( user.name, user.luck );

            var sortedDict = from entry in dic orderby entry.Value descending select entry;

            await ReplyAsync( $"```diff\n-{sortedDict.ElementAt( 0 ).Key} {new string( ' ', 7 - sortedDict.ElementAt( 0 ).Key.Length )} => [{sortedDict.ElementAt( 0 ).Value}%] de chance!\n```" );

            for ( int i = 1; i < sortedDict.Count(); i++ )
            {
                var sep = new string(' ', 8 - sortedDict.ElementAt(i).Key.Length);
                msg += $"{sortedDict.ElementAt( i ).Key} {sep} => [{sortedDict.ElementAt( i ).Value}%] de chance!\n";
            }

            await ReplyAsync( parser.format_message( msg ) );
        }

        [Command( "setj", true )]
        public async Task set()
        {
            var user = new user(Context.Message.Author);
            var users = parser.get_mentions(Context);
            var args = parser.parse<int>(Context.Message.ToString(), 1);

            if ( user.is_registered && user.is_admin )
            {
                foreach ( var target in users )
                {
                    target.jocker = args[0];
                    var jocker = target.jocker > 1 ? "jockers" : "jocker";
                    await ReplyAsync( parser.format_message( $"{target.name} a maintenant {target.jocker} {jocker}" ) );
                }
            }
        }

        [Command( "addj", true )]
        public async Task add()
        {
            var user = new user(Context.Message.Author);
            var users = parser.get_mentions(Context);
            var args = parser.parse<int>(Context.Message.ToString(), 1);

            if ( user.is_registered && user.is_admin )
            {
                foreach ( var target in users )
                {
                    target.jocker += args[0];
                    var jocker = target.jocker > 1 ? "jockers" : "jocker";
                    await ReplyAsync( parser.format_message( $"{target.name} a maintenant {target.jocker} {jocker}" ) );
                }
            }
        }

        [Command( "gc", true )]
        public async Task gc() => await cmp_curve();

        [Command( "graph_curve", true )]
        public async Task cmp_curve()
        {
            var info = await parser.get_graph_args(Context);
            if ( info.Item3 == null )
                return;

            var graph = utils.compare_graph_curve(info.Item1, info.Item2, info.Item3);
            graph.Save( "out.png" );

            await Context.Channel.SendFileAsync( "out.png" );
        }

        [Command( "gb", true )]
        public async Task gb() => await cmp_bar();

        [Command( "graph_bar", true )]
        public async Task cmp_bar()
        {
            var info = await parser.get_graph_args(Context);
            if ( info.Item3 == null )
                return;

            var graph = utils.compare_graph_bar(info.Item1, info.Item2, info.Item3);
            graph.Save( "out.png" );

            await Context.Channel.SendFileAsync( "out.png" );
        }

        [Command( "jocker", true )]
        public async Task jocker()
        {
            var user = new user(Context.Message.Author);

            if ( user.is_registered )
            {
                var jocker = user.jocker > 1 ? "jockers" : "jocker";
                await ReplyAsync( parser.format_message( $"Tu as {user.jocker} {jocker}" ) );
            }
        }

        [Command( "baisetagrossemere", true )]
        public async Task baisetagrossemere()
        {
            var user = new user(Context.Message.Author);

            if ( user.is_admin || (user.birth.Day == DateTime.Now.Day && user.birth.Day == DateTime.Now.Month) )
            {
                await ReplyAsync( "Merci maître !" );
                user.jocker += 10;
                var jocker = user.jocker > 1 ? "jockers" : "jocker";
                await ReplyAsync( parser.format_message( $"Tu as maintenant {user.jocker} {jocker}" ) );
            }
            else
            {
                await Context.Channel.SendFileAsync( "mirroir.png" );
                user.jocker -= 1;
                var jocker = user.jocker > 1 ? "jockers" : "jocker";
                await ReplyAsync( parser.format_message( $"Tu as maintenant {user.jocker} {jocker} sale fils de juifs" ) );
            }
        }

        [Command( "color", true )]
        public async Task set_color()
        {
            var user = new user(Context.Message.Author);
            var users = parser.get_mentions(Context);
            var args = parser.parse<int>(Context.Message.ToString(), 3);

            if ( user.is_registered )
            {
                if ( users.Length > 0 )
                    foreach ( var u in users )
                        await create_picture( u );

                else
                    await create_picture( user );

                async Task create_picture( user s_user )
                {
                    if ( args.Count() == 1 )
                        s_user.graph_color = args[0];
                    else if ( args.Count() == 3 )
                        s_user.graph_color = System.Drawing.Color.FromArgb( args[0], args[1], args[2] ).ToArgb();

                    var bmp = new Bitmap(200, 200);
                    using ( var gfx = Graphics.FromImage( bmp ) )
                    using ( var bsh = new SolidBrush( System.Drawing.Color.FromArgb( s_user.graph_color ) ) )
                        gfx.FillRectangle( bsh, 0, 0, 200, 200 );
                    bmp.Save( "user_color.png" );

                    await ReplyAsync( $"Couleur de {s_user.name}:" );
                    await Context.Channel.SendFileAsync( "user_color.png" );
                }
            }
        }

        [Command( "at", true )]
        public async Task at()
        {
            var args = parser.parse<int>(Context.Message.ToString());
            var users = parser.get_mentions(Context);

            if ( users.Length == 0 )
                users = users.Concat( new user[] { new user( Context.Message.Author ) } ).ToArray();

            var at_date = parser.to_date(args);
            var s_at_date = parser.date_to_string(at_date);
            var msg = default(string);

            foreach ( var user in users )
            {
                if ( !user.is_registered )
                    continue;

                var luck = user.luck_at(at_date);

                if ( at_date.Ticks > DateTime.Now.Ticks && user.id == new user( Context.Message.Author ).id )
                {
                    if ( user.jocker > 0 )
                    {
                        user.jocker--;
                        var jocker = user.jocker > 1 ? "jockers" : "jocker";
                        await ReplyAsync( parser.format_message( $"Il te reste {user.jocker} {jocker} !" ) );
                        var msg1 = $"Le {s_at_date}, ta chance sera de [{luck}%] !";
                        await ReplyAsync( parser.format_message( msg1 ) );
                    }
                    else
                    {
                        var msg1 = "Tu n'as plus de jocker, dommage !";
                        await ReplyAsync( parser.format_message( msg1 ) );
                    }
                    return;
                }
                else
                    msg += $"Le {s_at_date}, la chance de {user.name}{new string( ' ', 8 - user.name.Length )}etait de [{luck}%] !\n";
            }

            await ReplyAsync( parser.format_message( msg ) );
        }

        [Command( "luck", true )]
        public async Task luck()
        {
            var user = new user(Context.Message.Author);
            var args = parser.parse<int>(Context.Message.ToString());
            if ( user.is_registered )
            {
                await ReplyAsync( parser.format_message( $"Ta chance est de [{user.luck}%] !" ) );
            }
            else
                await ReplyAsync( $"*register [jour] [mois] [année] ?" );
        }

        [Command( "help", true )]
        public async Task help()
        {
            var cmds = new string[]
            {
            "LuckBot Helpers\n",
            "*command [params] {optional params}\n",
            "*register [day] [month] [year]                                                           | register using your birth date.",
            "*luck                                                                                    | retrieve your current luck for the present day.",
            "*jocker                                                                                  | retrieve your number of jockers.\n",
            "*color                                                                                   | retrieve your color for graphics.",
            "*color [red] [green] [blue] {user}                                                       | set your color with RGB conventions.",
            "*color [rbg hex code] {user}                                                             | set your color with RGB Hex Code conventions.\n",
            "*gc [X days] {user1} {user2} ..                                                          | graphics with curves of X number of last X days.",
            "*gc [from_day] [from_month] [from_year] [to_day] [to_month] [to_year] {user1} {user2} .. | graphics with curves between two dates.\n",
            "*gb [X days] {user1} {user2} ..                                                          | graphics with bars of X number of last X days.",
            "*gb [from_day] [from_month] [from_year] [to_day] [to_month] [to_year] {user1} {user2} .. | graphics with bars between two dates.\n",
            "*baisetagrossemere                                                                       | suprise motherfucker\n",
            };

            var commands = string.Join("\n", cmds);

            await ReplyAsync( parser.format_message( commands ) );
        }
    }

    class utils
    {
        public static Bitmap compare_graph_curve( DateTime firstday, DateTime lastday, user[] users )
        {
            //g settings
            GraphPane testg = new GraphPane();
            testg.Title.Text = "Taux chancéïque journalier";
            testg.LineType = LineType.Normal;

            //x axis
            testg.XAxis.Title.Text = "Date journaïtique";
            testg.XAxis.Type = AxisType.Date;
            testg.XAxis.Scale.Format = "dd-MMM-yy";
            testg.XAxis.Scale.MajorUnit = DateUnit.Day;
            testg.XAxis.Scale.MajorStep = 1;
            testg.XAxis.Scale.Min = new XDate( firstday.AddDays( -1 ) );
            testg.XAxis.Scale.Max = new XDate( lastday.AddDays( 1 ) );
            testg.XAxis.MajorTic.IsBetweenLabels = true;
            testg.XAxis.MinorTic.Size = 0;
            testg.XAxis.MajorTic.IsInside = false;
            testg.XAxis.MajorTic.IsOutside = true;
            //testg.BarSettings.Type = BarType.Cluster;

            //y axis
            testg.YAxis.Title.Text = "Pourcentage chancéïque (%)";
            testg.YAxis.Scale.Min = 0;
            testg.YAxis.Scale.Max = 100;

            //random color

            foreach ( var user in users )
            {
                double moyenne = 0;
                List<double> yluck = new List<double>();
                var xdays = new List<double>();

                for ( DateTime d = firstday; d <= lastday; d = d.AddDays( 1 ) )
                {
                    xdays.Add( new XDate( d ) );
                    yluck.Add( user.luck_at( d ) );
                    moyenne += user.luck_at( d );
                }
                var curve = testg.AddCurve("Chance pour " + user.name, xdays.ToArray(), yluck.ToArray(), System.Drawing.Color.FromArgb(user.graph_color));

                moyenne /= yluck.Count;
                double[] listmoy = Enumerable.Repeat(moyenne, yluck.Count + 2).ToArray();
                xdays.Insert( 0, new XDate( firstday.AddDays( -1 ) ) );
                xdays.Add( new XDate( lastday.AddDays( 1 ) ) );
                var moy = testg.AddCurve("moyenne chancéïque", xdays.ToArray(), listmoy, System.Drawing.Color.FromArgb(user.graph_color));
                moy.Line.Width = 2;
                moy.Symbol.Size = 0;

                curve.Line.Width = 4;
                curve.Symbol.Type = SymbolType.Diamond;
                curve.Symbol.Size = 1;
                curve.Symbol.Fill = new Fill( System.Drawing.Color.Orange );
                curve.Line.IsSmooth = true;
                curve.Line.SmoothTension = 0.2f;
            }

            //enregister bitmap
            var bm = new Bitmap(1, 1);
            var g = Graphics.FromImage(bm);
            testg.AxisChange( g );
            var bm2 = testg.GetImage(1920, 1080, 1000, true);
            bm2.Save( "out.png" );
            return bm2;
        }

        public static Bitmap compare_graph_bar( DateTime firstday, DateTime lastday, user[] users )
        {
            //g settings
            GraphPane testg = new GraphPane();
            testg.Title.Text = "Taux chancéïque journalier";
            testg.LineType = LineType.Normal;

            //x axis
            testg.XAxis.Title.Text = "Date journaïtique";
            testg.XAxis.Type = AxisType.Date;
            testg.XAxis.Scale.Format = "dd-MMM-yy";
            testg.XAxis.Scale.MajorUnit = DateUnit.Day;
            testg.XAxis.Scale.MajorStep = 1;
            testg.XAxis.Scale.Min = new XDate( firstday.AddDays( -1 ) );
            testg.XAxis.Scale.Max = new XDate( lastday.AddDays( 1 ) );
            testg.XAxis.MajorTic.IsBetweenLabels = true;
            testg.XAxis.MinorTic.Size = 0;
            testg.XAxis.MajorTic.IsInside = false;
            testg.XAxis.MajorTic.IsOutside = true;
            testg.BarSettings.Type = BarType.Cluster;

            //y axis
            testg.YAxis.Title.Text = "Pourcentage chancéïque (%)";
            testg.YAxis.Scale.Min = 0;
            testg.YAxis.Scale.Max = 100;

            //random color
            var xdays = new List<double>();
            foreach ( var user in users )
            {
                double moyenne = 0;
                List<double> yluck = new List<double>();
                for ( DateTime d = firstday; d <= lastday; d = d.AddDays( 1 ) )
                {
                    xdays.Add( new XDate( d ) );
                    yluck.Add( user.luck_at( d ) );
                    moyenne += user.luck_at( d );
                }
                var curve = testg.AddBar("Chance pour " + user.name, xdays.ToArray(), yluck.ToArray(), System.Drawing.Color.FromArgb(user.graph_color));
                moyenne /= yluck.Count;
                double[] listmoy = Enumerable.Repeat(moyenne, yluck.Count + 2).ToArray();

                xdays.Insert( 0, new XDate( firstday.AddDays( -1 ) ) );
                xdays.Add( new XDate( lastday.AddDays( 1 ) ) );
                var moy = testg.AddCurve("moyenne chancéïque", xdays.ToArray(), listmoy, System.Drawing.Color.FromArgb(user.graph_color));
                moy.Line.Width = 1;
                moy.Symbol.Size = 0;
            }

            //enregister bitmap
            var bm = new Bitmap(1, 1);
            var g = Graphics.FromImage(bm);
            testg.AxisChange( g );
            var bm2 = testg.GetImage(1920, 1080, 1000, true);
            bm2.Save( "out.png" );
            return bm2;
        }
    }
}
