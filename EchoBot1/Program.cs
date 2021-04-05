// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.11.1

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using System;

namespace Ulsolution.HrBot
{
    public class Program
    {
		private DiscordSocketClient _client;

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();

            //MainAsync().GetAwaiter().GetResult();
        }

        //public static void Main(string[] args)
        //{
        //          new Program().MainAsync().GetAwaiter().GetResult();
        //          CreateHostBuilder(args).Build().Run();
        //}


        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

		 

		//public async Task MainAsync()
		//{
		//	// When working with events that have Cacheable<IMessage, ulong> parameters,
		//	// you must enable the message cache in your config settings if you plan to
		//	// use the cached message entity. 
		//	var _config = new DiscordSocketConfig { MessageCacheSize = 100 };
		//	_client = new DiscordSocketClient(_config);

		//	await _client.LoginAsync(TokenType.Bot, "ODI4MjYzNTc2ODE5OTkwNTU5.YGnCkQ.ItHQwv-vzg_lX7hAGwUWbIxpo2U");
		//	await _client.StartAsync();

		//	_client.MessageUpdated += MessageUpdated;
		//	_client.Ready += () =>
		//	{
		//		Console.WriteLine("Bot is connected!");
		//		return Task.CompletedTask;
		//	};


		//    //await Task.Delay(-1);
		//}

		//private async Task MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel)
		//{
		//	// If the message was not in the cache, downloading it will result in getting a copy of `after`.
		//	var message = await before.GetOrDownloadAsync();
		//	Console.WriteLine($"{message} -> {after}");
		//}
		 

	}
}
