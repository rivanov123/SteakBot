﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using SteakBot.Core.Modules;

namespace SteakBot.Core
{
	public class Bot
	{
		private static readonly string discordBotToken = "";

		private CommandService _commands;
		private AudioService _audioService;
		private DiscordSocketClient _client;
		private IServiceProvider _services;

		public async Task RunAsync()
		{
			_client = new DiscordSocketClient();
			_client.Log += Log;

			_commands = new CommandService();
			_audioService = new AudioService();

			await _client.LoginAsync(TokenType.Bot, discordBotToken);
			await _client.StartAsync();

			_services = new ServiceCollection()
				.AddSingleton(_client)
				.AddSingleton(_commands)
				.AddSingleton(_audioService)
				.BuildServiceProvider();

			await InstallCommandsAsync();

			Console.ReadLine();

			await _client.LogoutAsync();
			await _client.StopAsync();

			Console.ReadLine();

			Console.WriteLine("Done");

			// Block this task until the program is closed.
			await Task.Delay(-1);

			//var webhookClient = new Discord.Webhook.DiscordWebhookClient(123, "", new DiscordRestConfig());
			//webhookClient.
		}

		private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}

		public async Task InstallCommandsAsync()
		{
			// Hook the MessageReceived Event into our Command Handler
			_client.MessageReceived += HandleCommandAsync;

			// Discover all of the commands in this assembly and load them.
			await _commands.AddModulesAsync(Assembly.GetExecutingAssembly());
		}

		private async Task HandleCommandAsync(SocketMessage messageParam)
		{
			// Don't process the command if it was a System Message
			var message = messageParam as SocketUserMessage;
			if (message == null)
				return;

			if (message.Source == MessageSource.Bot)
				return;

			// Create a number to track where the prefix ends and the command begins
			var argPos = 0;

			// Create a Command Context
			var context = new SocketCommandContext(_client, message);

			// Determine if the message is a command, based on if it starts with '!' or a mention prefix
			if (message.HasCharPrefix('!', ref argPos))// || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
			{
				if (await ManualCommandHandler.HandleCommandAsync(message))
				{
					return;
				}

				// Execute the command. (result does not indicate a return value, 
				// rather an object stating if the command executed successfully)
				var result = await _commands.ExecuteAsync(context, argPos, _services);
				if (!result.IsSuccess)
				{
					await context.Channel.SendMessageAsync(result.ErrorReason);
				}
			}
		}
	}
}
