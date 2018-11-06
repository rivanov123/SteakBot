﻿using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace SteakBot.Core.Modules
{
	public class HelpModule : ModuleBase<SocketCommandContext>
	{
		[Command("list")]
		public async Task Shrug()
		{
			await ReplyAsync(string.Join("\r\n", ManualCommandHandler.Commands.Select(x => $"`{x.Name}` - {x.Description}")));
		}

		[Command("help")]
		public async Task Help()
		{
			await ReplyAsync("`hi` - Bot says \"Hi\"");
			await ReplyAsync("`say <something>` - Bot says something");
			await ReplyAsync("`list` - Lists all available \"memes\"");
			await ReplyAsync("`quote` - Duuh, quotes...");
		}
	}
}
