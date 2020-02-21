using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.Reflection;
using Discord.Commands;
using Discord.WebSocket;
using System.IO;
using Microsoft.Extensions.DependencyInjection;

namespace LuckBot
{
    class bot_main
    {
        static void Main(string[] args)
        {
            if (!File.Exists("file_cfg.cfg"))
                File.Create("file_cfg.cfg").Close();

            var bot = new bot();
            bot.run_bot().GetAwaiter().GetResult();
        }
    }

    class bot
    {
        private DiscordSocketClient client;
        private CommandService command;
        private IServiceProvider service;


        public async Task run_bot()
        {
            client = new DiscordSocketClient();
            command = new CommandService();
            service = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(command)
                .BuildServiceProvider();

            var token = "NjY5NjI4ODU2MzQ4NDQyNjI0.Xiim4w.jH5wP-Ob_n-SS5dNnVPAfB-dZuE";

            client.Log += log;

            await get_commands();

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
            await Task.Delay(-1);
        }

        private Task log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        public async Task get_commands()
        {
            client.MessageReceived += run_cmd;
            await command.AddModulesAsync(Assembly.GetEntryAssembly(), service);
        }

        private async Task run_cmd(SocketMessage arg)
        {
            var message = (SocketUserMessage)arg;
            var context = new SocketCommandContext(client, message);
            if (message.Author.IsBot)
                return;

            var arg_pos = 0;
            if (message.HasStringPrefix("*", ref arg_pos))
            {
                var result = await command.ExecuteAsync(context, arg_pos, service);
                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
            }
        }
    }
}
