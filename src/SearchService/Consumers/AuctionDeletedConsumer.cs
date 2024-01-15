using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionDeletedConsumer() : IConsumer<AuctionDeleted>
{
	public async Task Consume(ConsumeContext<AuctionDeleted> context)
	{
		Console.WriteLine($"--> Consuming auction deleted: {context.Message.Id}");
		var res = await DB.DeleteAsync<Item>(context.Message.Id);
		if (!res.IsAcknowledged) throw new MessageException(typeof(AuctionDeleted), $"Problem deleting auction {context.Message.Id}");
	}
}